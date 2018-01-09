using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    internal interface IInterviewsExporter
    {
        void Export(QuestionnaireExportStructure questionnaireExportStructure, List<InterviewToExport> interviewIdsToExport, string basePath, IProgress<int> progress, CancellationToken cancellationToken);
    }

    internal class InterviewsExporter : IInterviewsExporter
    {
        private readonly IExportViewFactory exportViewFactory;
        private readonly IInterviewFactory interviewFactory;
        private readonly string dataFileExtension = "tab";

        private readonly ILogger logger;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly ICsvWriter csvWriter;
        private ITransactionManager TransactionManager => transactionManagerProvider.GetTransactionManager();
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IInterviewErrorsExporter errorsExporter;

        public InterviewsExporter(ILogger logger,
            InterviewDataExportSettings interviewDataExportSettings, 
            ICsvWriter csvWriter, 
            ITransactionManagerProvider plainTransactionManagerProvider,
            IInterviewErrorsExporter errorsExporter,
            IInterviewFactory interviewFactory,
            IExportViewFactory exportViewFactory)
        {
            this.exportViewFactory = exportViewFactory ?? throw new ArgumentNullException(nameof(exportViewFactory));
            this.interviewFactory = interviewFactory ?? throw new ArgumentNullException(nameof(interviewFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.interviewDataExportSettings = interviewDataExportSettings ?? throw new ArgumentNullException(nameof(interviewDataExportSettings));
            this.csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
            this.transactionManagerProvider = plainTransactionManagerProvider ?? throw new ArgumentNullException(nameof(plainTransactionManagerProvider));
            this.errorsExporter = errorsExporter ?? throw new ArgumentNullException(nameof(errorsExporter));
        }

        public virtual void Export(QuestionnaireExportStructure questionnaireExportStructure, List<InterviewToExport> interviewIdsToExport, string basePath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            this.DoExport(questionnaireExportStructure, basePath, interviewIdsToExport, progress, cancellationToken);
            stopwatch.Stop();
            this.logger.Info($"Export of {interviewIdsToExport.Count:N0} interview datas for questionnaire {new QuestionnaireIdentity(questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version)} finised. Took {stopwatch.Elapsed:c} to complete");
        }

        private void DoExport(QuestionnaireExportStructure questionnaireExportStructure, 
            string basePath, 
            List<InterviewToExport> interviewIdsToExport, 
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
                string filePath = Path.Combine(basePath, fileName);

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

            bool hasAtLeastOneRoster = questionnaireExportStructure.HeaderToLevelMap.Values.Any(x => x.LevelScopeVector.Count > 0);

            var errorsExportFilePath = Path.Combine(basePath, InterviewErrorsExporter.FileName);
            errorsExportFilePath = Path.ChangeExtension(errorsExportFilePath, dataFileExtension);
            this.errorsExporter.WriteHeader(hasAtLeastOneRoster, questionnaireExportStructure.MaxRosterDepth, errorsExportFilePath);
        }

        private void ExportInterviews(List<InterviewToExport> interviewIdsToExport, 
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
                    interviewToExport => {
                       cancellationToken.ThrowIfCancellationRequested();
                       InterviewExportedDataRecord exportedData = this.ExportSingleInterview(interviewToExport, questionnaireExportStructure, basePath);
                       exportBulk.Add(exportedData);

                       Interlocked.Increment(ref totalInterviewsProcessed);
                       progress.Report(totalInterviewsProcessed.PercentOf(interviewIdsToExport.Count));
                   });

                batchWatch.Stop();
                
                Stopwatch writeToFilesWatch = Stopwatch.StartNew();
                this.WriteInterviewDataToCsvFile(basePath, exportBulk.ToList());

                this.logger.Debug(string.Format("Exported {0:N0} in {3:g} interviews out of {1:N0}. for questionnaire {2}. Get data from db per batch: {4:g}. Processing per batch {5:g}. Write to files per batch {6:g}",
                    totalInterviewsProcessed,
                    interviewIdsToExport.Count,
                    new QuestionnaireIdentity(questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version),
                    batchWatch.Elapsed,
                    getDbDataStopwatch.Elapsed,
                    exportProcessingStopwatch.Elapsed,
                    writeToFilesWatch.Elapsed));
                getDbDataStopwatch.Reset();
                exportProcessingStopwatch.Reset();

            }

            progress.Report(100);
        }

        private void WriteInterviewDataToCsvFile(string basePath,
            List<InterviewExportedDataRecord> interviewsToDump)
        {
            var exportBulk = ConvertInterviewsToWriteBulk(interviewsToDump);
            this.WriteCsv(basePath, exportBulk);
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

                        exportBulk[levelName].Add(dataByLevel.EmptyIfNull().Split(ExportFileSettings.DataFileSeparator));
                    }
                }
            }

            return exportBulk;
        }

        private void WriteCsv(string basePath, Dictionary<string, List<string[]>> exportBulk)
        {
            foreach (var level in exportBulk.Keys)
            {
                var dataByTheLevelFilePath = Path.Combine(basePath,
                    this.CreateFormatDataFileName(level));

                this.csvWriter.WriteData(dataByTheLevelFilePath,
                    exportBulk[level],
                    ExportFileSettings.DataFileSeparator.ToString());
            }
        }

        private readonly Stopwatch getDbDataStopwatch = new Stopwatch();
        private readonly Stopwatch exportProcessingStopwatch = new Stopwatch();

        private InterviewExportedDataRecord ExportSingleInterview(InterviewToExport interviewToExport, QuestionnaireExportStructure exportStructure, string basePath)
        {
            getDbDataStopwatch.Start();
            var interview =  this.TransactionManager.ExecuteInQueryTransaction(() => interviewFactory.GetInterviewEntities(exportStructure.Identity, interviewToExport.Id));
            getDbDataStopwatch.Stop();

            exportProcessingStopwatch.Start();
            List<string[]> errors = errorsExporter.Export(exportStructure, interview, basePath);

            var interviewData = new InterviewData
            {
                Levels = interviewFactory.GetInterviewDataLevels(exportStructure.Identity, interview),
                InterviewId = interviewToExport.Id
            };
            InterviewDataExportView interviewDataExportView = exportViewFactory.CreateInterviewDataExportView(exportStructure, interviewData);
            InterviewExportedDataRecord exportedData = this.CreateInterviewExportedData(interviewDataExportView, interviewToExport);
            var dataFileSeparator = ExportFileSettings.DataFileSeparator.ToString();
            exportedData.Data[InterviewErrorsExporter.FileName] = errors.Select(x =>  string.Join(dataFileSeparator, x.Select(v => v?.Replace(dataFileSeparator, "")))).ToArray();
            exportProcessingStopwatch.Stop();
            return exportedData;
        }

        private InterviewExportedDataRecord CreateInterviewExportedData(InterviewDataExportView interviewDataExportView, InterviewToExport interviewId)
        {
            var interviewData = new Dictionary<string, string[]>(); // file name, array of rows

            var stringSeparator = ExportFileSettings.DataFileSeparator.ToString();
            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                var recordsByLevel = new List<string>();
                foreach (var interviewDataExportRecord in interviewDataExportLevelView.Records)
                {
                    var parametersToConcatenate = new List<string> { interviewDataExportRecord.RecordId };

                    parametersToConcatenate.AddRange(interviewDataExportRecord.ReferenceValues);

                    for (int i = 0; i < interviewDataExportRecord.Answers.Length; i++)
                    {
                        for (int j = 0; j < interviewDataExportRecord.Answers[i].Length; j++)
                        {
                            parametersToConcatenate.Add(interviewDataExportRecord.Answers[i][j] == null ? "" : interviewDataExportRecord.Answers[i][j]);
                        }
                    }

                    var systemVariableValues = new List<string>(interviewDataExportRecord.SystemVariableValues);
                    if (systemVariableValues.Count > 0) // main file?
                    {
                        var interviewKeyIndex = ServiceColumns.SystemVariables[ServiceVariableType.InterviewKey].Index;
                        if (systemVariableValues.Count < interviewKeyIndex + 1)
                            systemVariableValues.Add(interviewId.Key);
                        if (string.IsNullOrEmpty(systemVariableValues[interviewKeyIndex]))
                            systemVariableValues[interviewKeyIndex] = interviewId.Key;

                        systemVariableValues.Insert(ServiceColumns.SystemVariables[ServiceVariableType.HasAnyError].Index,
                            interviewId.HasErrors ? "1" : "0");
                        systemVariableValues.Insert(ServiceColumns.SystemVariables[ServiceVariableType.InterviewStatus].Index,
                            interviewId.Status.ToString());
                    }

                    parametersToConcatenate.AddRange(systemVariableValues);
                    parametersToConcatenate.AddRange(interviewDataExportRecord.ParentRecordIds);

                    if (systemVariableValues.Count == 0)
                    {
                        parametersToConcatenate.Add(interviewId.Key);
                    }

                    recordsByLevel.Add(string.Join(stringSeparator,
                            parametersToConcatenate.Select(v => v?.Replace(stringSeparator, ""))));
                }

                interviewData.Add(interviewDataExportLevelView.LevelName, recordsByLevel.ToArray());
            }

            var interviewExportedData = new InterviewExportedDataRecord
            {
                InterviewId = interviewId.Id.FormatGuid(),
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