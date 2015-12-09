using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
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

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    public class InterviewsExporter
    {
        private readonly string dataFileExtension = "tab";

        private readonly ITransactionManagerProvider transactionManager;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewDatas;
        private readonly IExportViewFactory exportViewFactory;
        private readonly ILogger logger;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly ICsvWriter csvWriter;

        public InterviewsExporter(ITransactionManagerProvider transactionManager, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries, 
            IFileSystemAccessor fileSystemAccessor, 
            IReadSideKeyValueStorage<InterviewData> interviewDatas, 
            IExportViewFactory exportViewFactory,
            ILogger logger,
            InterviewDataExportSettings interviewDataExportSettings,
            ICsvWriter csvWriter)
        {
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewDatas = interviewDatas;
            this.exportViewFactory = exportViewFactory;
            this.logger = logger;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.csvWriter = csvWriter;
        }

        public void ExportAll(QuestionnaireExportStructure questionnaireExportStructure, string basePath, IProgress<int> progress, CancellationToken cancellationToken)
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

        public void ExportApproved(QuestionnaireExportStructure questionnaireExportStructure, string basePath, IProgress<int> progress, CancellationToken cancellationToken)
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
                this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() => this.interviewSummaries.Query(_ => _.Count(expression)));

            List<Guid> interviewIdsToExport = new List<Guid>();
            while (interviewIdsToExport.Count < totalInterviewsToExport)
            {
                var ids = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() =>
                   this.interviewSummaries.Query(_ => _
                       .Where(expression)
                       .OrderBy(x => x.InterviewId)
                       .Select(x => x.InterviewId)
                       .Skip(interviewIdsToExport.Count)
                       .Take(this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery)
                       .ToList()));
                if (ids.Count == 0) break;

                cancellationToken.ThrowIfCancellationRequested();
                interviewIdsToExport.AddRange(ids);
            }

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

        private static object lockObject = new Object();

        private void ExportInterviews(List<Guid> interviewIdsToExport, 
            string basePath, 
            QuestionnaireExportStructure questionnaireExportStructure, 
            IProgress<int> progress, 
            CancellationToken cancellationToken)
        {
            int totalInterviewsProcessed = 0;

            foreach (var batchIds in interviewIdsToExport.Batch(this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery))
            {
                Dictionary<string, List<string[]>> exportBulk = new Dictionary<string, List<string[]>>();
                Parallel.ForEach(batchIds,
                   new ParallelOptions
                   {
                       CancellationToken = cancellationToken
                   },
                   interviewId => {
                       cancellationToken.ThrowIfCancellationRequested();
                       this.ExportSingleInterview(questionnaireExportStructure, interviewId, exportBulk);

                       Interlocked.Increment(ref totalInterviewsProcessed);
                       progress.Report(totalInterviewsProcessed.PercentOf(interviewIdsToExport.Count));
                   });

                this.WriteInterviewDataToCsvFile(basePath, questionnaireExportStructure, exportBulk);
            }

            progress.Report(100);
        }

        private void WriteInterviewDataToCsvFile(string basePath, 
            QuestionnaireExportStructure questionnaireExportStructure,
            Dictionary<string, List<string[]>> interviewsToDump)
        {
            foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                var dataByTheLevelFilePath = this.fileSystemAccessor.CombinePath(basePath,
                    this.CreateFormatDataFileName(level.LevelName));

                if (interviewsToDump.ContainsKey(level.LevelName))
                {
                    this.csvWriter.WriteData(dataByTheLevelFilePath,
                        interviewsToDump[level.LevelName],
                        ExportFileSettings.SeparatorOfExportedDataFile.ToString());
                }
            }
        }

        private void ExportSingleInterview(QuestionnaireExportStructure questionnaireExportStructure, 
            Guid interviewId,
             Dictionary<string, List<string[]>> exportBulk)
        {
            var interviewData =
                this.transactionManager.GetTransactionManager()
                    .ExecuteInQueryTransaction(() => this.interviewDatas.GetById(interviewId));

            InterviewDataExportView interviewExportStructure =
                this.exportViewFactory.CreateInterviewDataExportView(questionnaireExportStructure,
                    interviewData);

            InterviewExportedDataRecord exportedData = this.CreateInterviewExportedData(
                interviewExportStructure, questionnaireExportStructure.QuestionnaireId,
                questionnaireExportStructure.Version);


            foreach (var levelName in exportedData.Data.Keys)
            {
                foreach (var dataByLevel in exportedData.Data[levelName])
                {
                    if (!exportBulk.ContainsKey(levelName))
                    {
                        exportBulk.Add(levelName, new List<string[]>());
                    }

                    exportBulk[levelName].Add(dataByLevel.Split(ExportFileSettings.SeparatorOfExportedDataFile));
                }
            }
        }

        private InterviewExportedDataRecord CreateInterviewExportedData(InterviewDataExportView interviewDataExportView, Guid questionnaireId, long questionnaireVersion)
        {
            var interviewData = new Dictionary<string, string[]>();

            var stringSeparator = ExportFileSettings.SeparatorOfExportedDataFile.ToString();
            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                var recordsByLevel = new List<string>();
                foreach (var interviewDataExportRecord in interviewDataExportLevelView.Records)
                {
                    var parametersToConcatenate = new List<string> { interviewDataExportRecord.RecordId };

                    parametersToConcatenate.AddRange(interviewDataExportRecord.ReferenceValues);

                    foreach (var exportedQuestion in interviewDataExportRecord.Questions)
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
                InterviewId = interviewDataExportView.InterviewId.FormatGuid(),
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
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