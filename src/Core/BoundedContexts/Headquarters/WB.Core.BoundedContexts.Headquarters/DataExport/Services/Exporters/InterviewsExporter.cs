﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    internal class InterviewsExporter
    {
        private readonly string dataFileExtension = "tab";

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger logger;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly ICsvWriter csvWriter;
        private readonly InterviewExportredDataRowReader rowReader;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;
        private ITransactionManager TransactionManager => transactionManagerProvider.GetTransactionManager();
        private readonly ITransactionManagerProvider transactionManagerProvider;

        protected InterviewsExporter()
        {
        }

        public InterviewsExporter(IFileSystemAccessor fileSystemAccessor,
            ILogger logger,
            InterviewDataExportSettings interviewDataExportSettings, 
            ICsvWriter csvWriter,
            InterviewExportredDataRowReader rowReader,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries, 
            ITransactionManagerProvider plainTransactionManagerProvider)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.csvWriter = csvWriter;
            this.rowReader = rowReader;
            this.interviewSummaries = interviewSummaries;
            this.transactionManagerProvider = plainTransactionManagerProvider;
        }

        public virtual void Export(QuestionnaireExportStructure questionnaireExportStructure, List<Guid> interviewIdsToExport, string basePath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            this.DoExport(questionnaireExportStructure, basePath, interviewIdsToExport, progress, cancellationToken);
            stopwatch.Stop();
            this.logger.Info($"Export of {interviewIdsToExport.Count:N0} interview datas for questionnaire {new QuestionnaireIdentity(questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version)} finised. Took {stopwatch.Elapsed:c} to complete");
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
                string fileName = this.CreateFormatDataFileName(level.LevelName);
                string filePath = this.fileSystemAccessor.CombinePath(basePath, fileName);

                List<string> interviewLevelHeader = new List<string> { level.LevelIdColumnName };

                if (level.IsTextListScope)
                {
                    interviewLevelHeader.AddRange(level.ReferencedNames);
                }

                foreach (IExportedHeaderItem question in level.HeaderItems.Values)
                {
                    interviewLevelHeader.AddRange(question.ColumnNames);
                }

                if (level.LevelScopeVector.Length == 0)
                {
                    interviewLevelHeader.AddRange(ServiceColumns.SystemVariables.Values.Select(systemVariable => systemVariable.VariableExportColumnName));
                }

                interviewLevelHeader.AddRange(questionnaireExportStructure.GetAllParentColumnNamesForLevel(level.LevelScopeVector));
                
                this.csvWriter.WriteData(filePath, new[] { interviewLevelHeader.ToArray() }, ExportFileSettings.DataFileSeparator.ToString());
            }
        }

        private void ExportInterviews(List<Guid> interviewIdsToExport, 
            string basePath, 
            QuestionnaireExportStructure questionnaireExportStructure, 
            IProgress<int> progress, 
            CancellationToken cancellationToken)
        {
            long totalInterviewsProcessed = 0;
            
            foreach (var batchIds in interviewIdsToExport.Batch(this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery))
            {
                Stopwatch batchWatch = Stopwatch.StartNew();

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
                this.logger.Debug(string.Format("Exported {0:N0} in {3:g} interviews out of {1:N0} for questionnaire {2}",
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
                            .Split(ExportFileSettings.DataFileSeparator));
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
                        ExportFileSettings.DataFileSeparator.ToString());
                }
            }
        }

        private InterviewExportedDataRecord ExportSingleInterview(Guid interviewId)
        {
            var records = this.rowReader.ReadExportDataForInterview(interviewId);
            var interviewExportStructure = InterviewDataExportView.CreateFromRecords(interviewId, records);

            InterviewExportedDataRecord exportedData = this.CreateInterviewExportedData(interviewExportStructure, interviewId);

            return exportedData;
        }

        private InterviewExportedDataRecord CreateInterviewExportedData(InterviewDataExportView interviewDataExportView, Guid interviewId)
        {
            var interviewSummary = this.TransactionManager.ExecuteInQueryTransaction(() => 
                interviewSummaries.Query(_ =>
                    _.Where(i => i.SummaryId == interviewId.FormatGuid()).Select(i => new
                    {
                        i.Key,
                        i.HasErrors,
                        i.Status
                    }).FirstOrDefault()
                ));
            var interviewKey = interviewSummary.Key;
            var interviewData = new Dictionary<string, string[]>(); // file name, array of rows

            var stringSeparator = ExportFileSettings.DataFileSeparator.ToString();
            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                var recordsByLevel = new List<string>();
                foreach (var interviewDataExportRecord in interviewDataExportLevelView.Records)
                {
                    var parametersToConcatenate = new List<string> { interviewDataExportRecord.RecordId };

                    parametersToConcatenate.AddRange(interviewDataExportRecord.ReferenceValues);

                    foreach (var answer in interviewDataExportRecord.GetPlainAnswers())
                    {
                        parametersToConcatenate.AddRange(answer.Select(itemValue => string.IsNullOrEmpty(itemValue) ? "" : itemValue));
                    }

                    var systemVariableValues = new List<string>(interviewDataExportRecord.SystemVariableValues);
                    if (systemVariableValues.Count > 0) // main file?
                    {
                        var interviewKeyIndex = ServiceColumns.SystemVariables[ServiceVariableType.InterviewKey].Index;
                        if (systemVariableValues.Count < interviewKeyIndex + 1)
                            systemVariableValues.Add(interviewKey);
                        if (string.IsNullOrEmpty(systemVariableValues[interviewKeyIndex]))
                            systemVariableValues[interviewKeyIndex] = interviewKey;

                        systemVariableValues.Insert(ServiceColumns.SystemVariables[ServiceVariableType.HasAnyError].Index,
                            interviewSummary.HasErrors.ToString());
                        systemVariableValues.Insert(ServiceColumns.SystemVariables[ServiceVariableType.InterviewStatus].Index,
                            interviewSummary.Status.ToString());
                    }

                    parametersToConcatenate.AddRange(systemVariableValues);
                    parametersToConcatenate.AddRange(interviewDataExportRecord.ParentRecordIds);
                    if (systemVariableValues.Count == 0)
                    {
                        parametersToConcatenate.Add(interviewKey);
                    }

                    recordsByLevel.Add(string.Join(stringSeparator,
                            parametersToConcatenate.Select(v => v?.Replace(stringSeparator, ""))));
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
            return $"{fileName}.{dataFileExtension}";
        }
    }
}