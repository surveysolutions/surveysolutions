using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Dapper;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Metadata;
using NHibernate.Persister.Entity;
using Ninject;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveySolutions.Implementation.ServiceVariables;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    internal class InterviewsExporter
    {
        private readonly string dataFileExtension = "tab";

        private readonly ITransactionManagerProvider transactionManager;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;
        private readonly ILogger logger;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly ICsvWriter csvWriter;
        private readonly ISessionFactory sessionFactory;

        protected InterviewsExporter()
        {
        }

        public InterviewsExporter(ITransactionManagerProvider transactionManager, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries, 
            IFileSystemAccessor fileSystemAccessor,
            ILogger logger,
            InterviewDataExportSettings interviewDataExportSettings, 
            ICsvWriter csvWriter,
            [Named(PostgresReadSideModule.ReadSideSessionFactoryName)]ISessionFactory sessionFactory)
        {
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.csvWriter = csvWriter;
            this.sessionFactory = sessionFactory;
        }

        public virtual void ExportAll(QuestionnaireExportStructure questionnaireExportStructure, string basePath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version);
            this.logger.Info($"Export all interviews for questionnaire {questionnaireIdentity} started");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Expression<Func<InterviewSummary, bool>> expression = x => x.QuestionnaireId == questionnaireExportStructure.QuestionnaireId &&
                                         x.QuestionnaireVersion == questionnaireExportStructure.Version &&
                                         !x.IsDeleted;

            this.Export(questionnaireExportStructure, basePath, progress, cancellationToken, expression);
            stopwatch.Stop();
            this.logger.Info($"Export all interviews for questionnaire {questionnaireIdentity} finised. Took {stopwatch.Elapsed:c} to complete");
        }

        public virtual  void ExportApproved(QuestionnaireExportStructure questionnaireExportStructure, string basePath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version);

            Expression<Func<InterviewSummary, bool>> expression = x => x.QuestionnaireId == questionnaireExportStructure.QuestionnaireId && 
                                         x.QuestionnaireVersion == questionnaireExportStructure.Version &&
                                         !x.IsDeleted &&
                                         x.Status == InterviewStatus.ApprovedByHeadquarters;

            this.logger.Info($"Export approved interviews data for questionnaire {questionnaireIdentity} started");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            this.Export(questionnaireExportStructure, basePath, progress, cancellationToken, expression);
            stopwatch.Stop();

            this.logger.Info($"Export approved interviews data for questionnaire {questionnaireIdentity} finised. Took {stopwatch.Elapsed:c} to complete");
        }

        private void Export(QuestionnaireExportStructure questionnaireExportStructure, string basePath, IProgress<int> progress, CancellationToken cancellationToken,
            Expression<Func<InterviewSummary, bool>> expression)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int totalInterviewsToExport =
                this.transactionManager.GetTransactionManager()
                    .ExecuteInQueryTransaction(() => this.interviewSummaries.Query(_ => _.Count(expression)));

            this.logger.Info("Receiving interview Ids to export");
            List<Guid> interviewIdsToExport = new List<Guid>();

            Stopwatch idsWatch = new Stopwatch();
            idsWatch.Start();
            while (interviewIdsToExport.Count < totalInterviewsToExport)
            {
                var ids = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() =>
                    this.interviewSummaries.Query(_ => _
                        .Where(expression)
                        .OrderBy(x => x.InterviewId)
                        .Select(x => x.InterviewId)
                        .Skip(interviewIdsToExport.Count)
                        .Take(20000)
                        .ToList()));
                if (ids.Count == 0) break;

                cancellationToken.ThrowIfCancellationRequested();
                interviewIdsToExport.AddRange(ids);
                this.logger.Info($"Received {interviewIdsToExport.Count:n0} interview interview ids.");
            }

            this.logger.Info($"Starting export of {interviewIdsToExport.Count:n0} interviews. Took {idsWatch.Elapsed:c} to receive list of ids.");
            
            cancellationToken.ThrowIfCancellationRequested();
            this.DoExport(questionnaireExportStructure, basePath, interviewIdsToExport, progress, cancellationToken);
        }

        private void DoExport(QuestionnaireExportStructure questionnaireExportStructure, 
            string basePath, 
            List<Guid> interviewIdsToExport, 
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            
            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
            this.ExportInterviews(interviewIdsToExport, basePath, questionnaireExportStructure, progress, cancellationToken);
        }

        private void CreateDataSchemaForInterviewsInTabular(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                string dataByTheLevelFilePath =
                    this.fileSystemAccessor.CombinePath(basePath, CreateFormatDataFileName(level.LevelName));

                List<string> interviewLevelHeader = new List<string> { level.LevelIdColumnName };

                if (level.IsTextListScope)
                {
                    interviewLevelHeader.AddRange(level.ReferencedNames);
                }

                foreach (ExportedHeaderItem question in level.HeaderItems.Values)
                {
                    interviewLevelHeader.AddRange(question.ColumnNames);
                }

                if (level.LevelScopeVector.Length == 0)
                {
                    interviewLevelHeader.AddRange(ServiceColumns.SystemVariables.Select(systemVariable => systemVariable.VariableExportColumnName));
                }

                for (int i = 0; i < level.LevelScopeVector.Length; i++)
                {
                    interviewLevelHeader.Add($"{ServiceColumns.ParentId}{i + 1}");
                }

                this.csvWriter.WriteData(dataByTheLevelFilePath, new[] { interviewLevelHeader.ToArray() }, ExportFileSettings.SeparatorOfExportedDataFile.ToString());
            }
        }

        private void ExportInterviews(List<Guid> interviewIdsToExport, 
            string basePath, 
            QuestionnaireExportStructure questionnaireExportStructure, 
            IProgress<int> progress, 
            CancellationToken cancellationToken)
        {
            int totalInterviewsProcessed = 0;
            
            
            foreach (var batchIds in interviewIdsToExport.Batch(this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery))
            {
                Stopwatch batchWatch = new Stopwatch();
                batchWatch.Start();

                ConcurrentBag<InterviewExportedDataRecord> exportBulk = new ConcurrentBag<InterviewExportedDataRecord>();
                Parallel.ForEach(batchIds,
                   new ParallelOptions
                   {
                       CancellationToken = cancellationToken,
                       MaxDegreeOfParallelism = this.interviewDataExportSettings.InterviewsExportParallelTasksLimit
                   },
                   interviewId => {
                       cancellationToken.ThrowIfCancellationRequested();
                       InterviewExportedDataRecord exportedData = this.ExportSingleInterview(interviewId);
                       exportBulk.Add(exportedData);

                       Interlocked.Increment(ref totalInterviewsProcessed);
                       progress.Report(totalInterviewsProcessed.PercentOf(interviewIdsToExport.Count));
                   });

                batchWatch.Stop();
                this.logger.Info(string.Format("Exported {0:N0} in {3:c} interviews out of {1:N0} for questionnaire {2}",
                    totalInterviewsProcessed,
                    interviewIdsToExport.Count,
                    new QuestionnaireIdentity(questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version),
                    batchWatch.Elapsed));

                this.WriteInterviewDataToCsvFile(basePath, questionnaireExportStructure, exportBulk.ToList());
            }

            progress.Report(100);
        }

        private void WriteInterviewDataToCsvFile(string basePath, 
            QuestionnaireExportStructure questionnaireExportStructure,
            List<InterviewExportedDataRecord> interviewsToDump)
        {
            var exportBulk = ConvertInterviewsToWriteBulk(interviewsToDump);
            this.WriteCsv(basePath, questionnaireExportStructure, exportBulk);
        }

        private static Dictionary<string, List<string[]>> ConvertInterviewsToWriteBulk(List<InterviewExportedDataRecord> interviewsToDump)
        {
            Dictionary<string, List<string[]>> exportBulk = new Dictionary<string, List<string[]>>();

            foreach (var interviewExportedDataRecord in interviewsToDump)
            {
                foreach (var levelName in interviewExportedDataRecord.Data.Keys)
                {
                    foreach (var dataByLevel in interviewExportedDataRecord.Data[levelName])
                    {
                        if (!exportBulk.ContainsKey(levelName))
                        {
                            exportBulk.Add(levelName, new List<string[]>());
                        }

                        exportBulk[levelName].Add(dataByLevel.EmptyIfNull()
                            .Split(ExportFileSettings.SeparatorOfExportedDataFile));
                    }
                }
            }

            return exportBulk;
        }

        private void WriteCsv(string basePath, QuestionnaireExportStructure questionnaireExportStructure, Dictionary<string, List<string[]>> exportBulk)
        {
            foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                var dataByTheLevelFilePath = this.fileSystemAccessor.CombinePath(basePath,
                    this.CreateFormatDataFileName(level.LevelName));

                if (exportBulk.ContainsKey(level.LevelName))
                {
                    this.csvWriter.WriteData(dataByTheLevelFilePath,
                        exportBulk[level.LevelName],
                        ExportFileSettings.SeparatorOfExportedDataFile.ToString());
                }
            }
        }

        private InterviewExportedDataRecord ExportSingleInterview(Guid interviewId)
        {
            IList<InterviewDataExportRecord> records = null;

            using (var session = this.sessionFactory.OpenStatelessSession())
            {
                records = session.Connection.Query<InterviewDataExportRecord>("SELECT * FROM interviewdataexportrecords WHERE interviewid = @interviewId", 
                                                                              new {interviewId = interviewId})
                                            .ToList();
            }

            var interviewExportStructure = InterviewDataExportView.CreateFromRecords(interviewId, records);

            InterviewExportedDataRecord exportedData = this.CreateInterviewExportedData(interviewExportStructure, interviewId);

            return exportedData;
        }

        private InterviewExportedDataRecord CreateInterviewExportedData(InterviewDataExportView interviewDataExportView, Guid interviewId)
        {
            var interviewData = new Dictionary<string, string[]>(); // file name, array of rows

            var stringSeparator = ExportFileSettings.SeparatorOfExportedDataFile.ToString();
            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                var recordsByLevel = new List<string>();
                foreach (var interviewDataExportRecord in interviewDataExportLevelView.Records)
                {
                    var parametersToConcatenate = new List<string> { interviewDataExportRecord.RecordId };

                    parametersToConcatenate.AddRange(interviewDataExportRecord.ReferenceValues);

                    foreach (var exportedQuestion in interviewDataExportRecord.GetQuestions())
                    {
                        parametersToConcatenate.AddRange(exportedQuestion.Answers.Select(itemValue => string.IsNullOrEmpty(itemValue) ? "" : itemValue));
                    }

                    parametersToConcatenate.AddRange(interviewDataExportRecord.SystemVariableValues);
                    parametersToConcatenate.AddRange(interviewDataExportRecord.ParentRecordIds);

                    recordsByLevel.Add(string.Join(stringSeparator,
                            parametersToConcatenate.Select(v => v.Replace(stringSeparator, ""))));
                }

                interviewData.Add(interviewDataExportLevelView.LevelName, recordsByLevel.ToArray());
            }

            var interviewExportedData = new InterviewExportedDataRecord
            {
                InterviewId = interviewId.FormatGuid(),
                Data = interviewData,
            };

            return interviewExportedData;
        }

        private string CreateFormatDataFileName(string fileName)
        {
            return String.Format("{0}.{1}", fileName, dataFileExtension);
        }
    }
}