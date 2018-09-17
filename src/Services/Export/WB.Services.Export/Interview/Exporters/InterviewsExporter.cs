using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Tenant;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Interview.Exporters
{
    [Localizable(false)]
    internal class InterviewsExporter : IInterviewsExporter
    {
        public Task Export(TenantInfo tenant, 
            QuestionnaireExportStructure questionnaireExportStructure,
            QuestionnaireDocument questionnaire,
            List<InterviewToExport> interviewsToExport,
            string basePath, 
            Progress<int> progress, 
            CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            this.DoExport(questionnaireExportStructure, questionnaire, basePath, interviewsToExport, progress, cancellationToken);
            stopwatch.Stop();
            this.logger.Log(LogLevel.Information, $"Export of {interviewsToExport.Count:N0} interview datas for questionnaire" +
                                                  $" {questionnaireExportStructure.QuestionnaireId} finised. Took {stopwatch.Elapsed:c} to complete");
            return Task.CompletedTask;
        }

        private readonly IInterviewFactory interviewFactory;
        private readonly string dataFileExtension = "tab";

        private readonly ILogger<InterviewsExporter> logger;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly ICsvWriter csvWriter;
        private readonly IInterviewErrorsExporter errorsExporter;
        private readonly IExportQuestionService exportQuestionService;

        public InterviewsExporter(ILogger<InterviewsExporter> logger,
            InterviewDataExportSettings interviewDataExportSettings,
            ICsvWriter csvWriter,
            IInterviewErrorsExporter errorsExporter,
            IInterviewFactory interviewFactory, 
            IExportQuestionService exportQuestionService)
        {
            this.exportQuestionService = exportQuestionService;
            this.interviewFactory = interviewFactory ?? throw new ArgumentNullException(nameof(interviewFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.interviewDataExportSettings = interviewDataExportSettings ?? throw new ArgumentNullException(nameof(interviewDataExportSettings));
            this.csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
            this.errorsExporter = errorsExporter ?? throw new ArgumentNullException(nameof(errorsExporter));
        }

        
        private void DoExport(QuestionnaireExportStructure questionnaireExportStructure,
            QuestionnaireDocument questionnaire,
            string basePath,
            List<InterviewToExport> interviewIdsToExport,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
            this.ExportInterviews(interviewIdsToExport, basePath, questionnaireExportStructure, questionnaire, progress, cancellationToken);
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
                    interviewLevelHeader.AddRange(question.ColumnHeaders.Select(x => x.Name).ToArray());
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

        void ReadInterviewsFromDatabase(BlockingCollection<List<InterviewToExport>> interviewsToProcess,
            List<InterviewToExport> interviewIdsToExport, int batchSize, Action<string> log,
            CancellationToken cancellationToken)
        {
            try
            {
                foreach (IEnumerable<InterviewToExport> batchIds in interviewIdsToExport.Batch(batchSize))
                {
                    var batch = batchIds.ToDictionary(b => b.Id);

                    getDbDataStopwatch.Restart();

                    IEnumerable<InterviewEntity> interviews = interviewFactory.GetInterviewEntities(batch.Keys);

                    foreach (var interviewEntity in interviews)
                    {
                        batch[interviewEntity.InterviewId].Entities.Add(interviewEntity);
                    }

                    getDbDataStopwatch.Stop();

                    interviewsToProcess.Add(batch.Values.ToList(), cancellationToken);
                }
            }
            finally
            {
                interviewsToProcess.CompleteAdding();
            }
        }

        private void ExportInterviews(List<InterviewToExport> interviewIdsToExport,
            string basePath,
            QuestionnaireExportStructure questionnaireExportStructure,
            QuestionnaireDocument questionnaire,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            long totalInterviewsProcessed = 0;
            var batchSize = this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery;

            void LogDebug(string task)
            {
                this.logger.Log(LogLevel.Debug, $"Interview export for { questionnaireExportStructure.QuestionnaireId}: {task}");
            }

            // producer/consumer queue with backpressure limitation
            // it will block collection till there is 3 batches in queue, but make sure that we don't pause DB read for data processing
            var interviewsToProcess = new BlockingCollection<List<InterviewToExport>>(3);

            var dbReaderTask = Task.Factory.StartNew(
                () => ReadInterviewsFromDatabase(interviewsToProcess, interviewIdsToExport, batchSize, LogDebug, cancellationToken),
                cancellationToken);

            var exportStopwatch = Stopwatch.StartNew();

            foreach (var batch in interviewsToProcess.GetConsumingEnumerable(cancellationToken))
            {
                var exportBulk = new List<InterviewExportedDataRecord>();
                Stopwatch batchWatch = Stopwatch.StartNew();

                foreach (var interview in batch)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    exportBulk.Add(this.ExportSingleInterview(interview,
                        interview.Entities,
                        questionnaireExportStructure,
                        questionnaire,
                        basePath));

                    interview.Entities.Clear(); // needed to free ram as yearly as possible

                    ++totalInterviewsProcessed;
                    progress.Report(totalInterviewsProcessed.PercentOf(interviewIdsToExport.Count));
                }

                batchWatch.Stop();
                Stopwatch writeToFilesWatch = Stopwatch.StartNew();

                this.WriteInterviewDataToCsvFile(basePath, exportBulk);

                LogDebug($"Exported {totalInterviewsProcessed:N0} of {interviewIdsToExport.Count:N0} interviews in {batchWatch.Elapsed:g}. " +
                         $"Write to file: {writeToFilesWatch.Elapsed:g}");
            }

            dbReaderTask.Wait(cancellationToken);

            if (dbReaderTask.IsFaulted && dbReaderTask.Exception != null)
            {
                throw dbReaderTask.Exception;
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

        private InterviewExportedDataRecord ExportSingleInterview(InterviewToExport interviewToExport,
            List<InterviewEntity> interview, QuestionnaireExportStructure exportStructure, QuestionnaireDocument questionnaire, string basePath)
        {
            exportProcessingStopwatch.Start();
            List<string[]> errors = errorsExporter.Export(exportStructure, interview, basePath, interviewToExport.Key);

            var interviewData = new InterviewData
            {
                Levels = interviewFactory.GetInterviewDataLevels(questionnaire, interview),
                InterviewId = interviewToExport.Id
            };
            InterviewDataExportView interviewDataExportView = CreateInterviewDataExportView(exportStructure, interviewData, questionnaire);
            InterviewExportedDataRecord exportedData = this.CreateInterviewExportedData(interviewDataExportView, interviewToExport);
            var dataFileSeparator = ExportFileSettings.DataFileSeparator.ToString();
            exportedData.Data[InterviewErrorsExporter.FileName] = errors.Select(x => string.Join(dataFileSeparator, x.Select(v => v?.Replace(dataFileSeparator, "")))).ToArray();
            exportProcessingStopwatch.Stop();
            return exportedData;
        }

        private InterviewDataExportView CreateInterviewDataExportView(QuestionnaireExportStructure exportStructure,
            InterviewData interview, QuestionnaireDocument questionnaire)
        {
            var interviewDataExportLevelViews = new List<InterviewDataExportLevelView>();

            foreach (var exportStructureForLevel in exportStructure.HeaderToLevelMap.Values)
            {
                var interviewDataExportRecords = this.BuildRecordsForHeader(interview, exportStructureForLevel, questionnaire);

                var interviewDataExportLevelView = new InterviewDataExportLevelView(
                    exportStructureForLevel.LevelScopeVector, 
                    exportStructureForLevel.LevelName,
                    interviewDataExportRecords);

                interviewDataExportLevelViews.Add(interviewDataExportLevelView);
            }

            return new InterviewDataExportView(interview.InterviewId, interviewDataExportLevelViews.ToArray());
        }

        private InterviewDataExportRecord[] BuildRecordsForHeader(InterviewData interview,
            HeaderStructureForLevel headerStructureForLevel, QuestionnaireDocument questionnaire)
        {
            var dataRecords = new List<InterviewDataExportRecord>();

            var interviewDataByLevels = this.GetLevelsFromInterview(interview, headerStructureForLevel.LevelScopeVector);

            foreach (InterviewLevel dataByLevel in interviewDataByLevels)
            {
                var vectorLength = dataByLevel.RosterVector.Length;

                string recordId = interview.InterviewId.FormatGuid();
                
                string[] parentRecordIds = new string[vectorLength];
                string[] systemVariableValues = Array.Empty<string>();

                if (vectorLength == 0)
                    systemVariableValues = this.GetSystemValues(interview, ServiceColumns.SystemVariables.Values);
                else
                {
                    var rosterIndexAdjustment = headerStructureForLevel.LevelScopeVector
                        .Select(x => questionnaire.IsIntegerQuestion(x) ? 1 : 0)
                        .ToArray();
                    
                    recordId = (dataByLevel.RosterVector.Last() + rosterIndexAdjustment.Last())
                        .ToString(CultureInfo.InvariantCulture);

                    parentRecordIds[0] = interview.InterviewId.FormatGuid();
                    for (int i = 0; i < vectorLength - 1; i++)
                    {
                        parentRecordIds[i + 1] = (dataByLevel.RosterVector[i] + rosterIndexAdjustment[i]).ToString(CultureInfo.InvariantCulture);
                    }

                    parentRecordIds = parentRecordIds.Reverse().ToArray();
                }

                string[] referenceValues = Array.Empty<string>();

                if (headerStructureForLevel.IsTextListScope)
                {
                    referenceValues = new[]
                    {
                        this.GetTextValueForTextListQuestion(interview, dataByLevel.RosterVector, headerStructureForLevel.LevelScopeVector.Last())
                    };
                }

                string[][] questionsForExport = this.GetExportValues(dataByLevel, headerStructureForLevel);

                dataRecords.Add(new InterviewDataExportRecord(recordId,
                    referenceValues,
                    parentRecordIds,
                    systemVariableValues)
                {
                    Answers = questionsForExport
                });
            }

            return dataRecords.ToArray();
        }

        private string[][] GetExportValues(InterviewLevel interviewLevel, HeaderStructureForLevel headerStructureForLevel)
        {
            var result = new List<string[]>();
            foreach (var headerItem in headerStructureForLevel.HeaderItems.Values)
            {
                var questionHeaderItem = headerItem as ExportedQuestionHeaderItem;
                var variableHeaderItem = headerItem as ExportedVariableHeaderItem;

                if (questionHeaderItem != null)
                { 
                    var question = interviewLevel.QuestionsSearchCache.ContainsKey(headerItem.PublicKey) 
                        ? interviewLevel.QuestionsSearchCache[headerItem.PublicKey] 
                        : null;
                    var exportedQuestion = exportQuestionService.GetExportedQuestion(question, questionHeaderItem);
                    result.Add(exportedQuestion);
                }
                else if (variableHeaderItem != null)
                {
                    var variable = interviewLevel.Variables.ContainsKey(headerItem.PublicKey)
                        ? interviewLevel.Variables[headerItem.PublicKey]
                        : null;
                    var isDisabled = interviewLevel.DisabledVariables.Contains(headerItem.PublicKey);
                    var exportedVariable = exportQuestionService.GetExportedVariable(variable, variableHeaderItem, isDisabled);
                    result.Add(exportedVariable);
                }
                else
                {
                    throw  new ArgumentException("Unknown export header");
                }
            }
            return result.ToArray();
        }


        private string GetTextValueForTextListQuestion(InterviewData interview, decimal[] rosterVector, Guid id)
        {
            decimal itemToSearch = rosterVector.Last();

            for (var i = 1; i <= rosterVector.Length; i++)
            {
                var levelForVector =
                    interview.Levels.GetOrNull(
                        this.CreateLevelIdFromPropagationVector(rosterVector.Take(rosterVector.Length - i).ToArray()));

                var questionToCheck = levelForVector?.QuestionsSearchCache.GetOrNull(id);

                if (questionToCheck == null)
                    continue;


                InterviewTextListAnswer item = null;
                var listAnswers = questionToCheck.AsList;
                if (listAnswers != null)
                {
                    item = listAnswers.FirstOrDefault(a => a.Value == itemToSearch);
                }

                return item != null ? item.Answer : string.Empty;
            }

            return string.Empty;
        }

        private string CreateLevelIdFromPropagationVector(decimal[] vector)
        {
            return vector.Length == 0 ? "#" : vector.ToString();
        }

        private List<InterviewLevel> GetLevelsFromInterview(InterviewData interview, ValueVector<Guid> levelVector)
        {
            if (!levelVector.Any())
            {
                var levels = interview.Levels.Values.Where(level => level.ScopeVectors.ContainsKey(new ValueVector<Guid>())).ToList();
                return levels.Any() ? levels : new List<InterviewLevel> {new  InterviewLevel() {RosterVector = new decimal[0]}} ;
            }
            return interview.Levels.Values.Where(level => level.ScopeVectors.ContainsKey(levelVector)).ToList();
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
                            interviewId.ErrorsCount.ToString());
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

        private string[] GetSystemValues(InterviewData interview, IEnumerable<ServiceVariable> variables)
        {
            List<string> values = new List<string>();

            foreach (var header in variables)
            {
                values.Add(this.GetSystemValue(interview, header));
            }
            return values.ToArray();

        }

        
        private string GetSystemValue(InterviewData interview, ServiceVariable serviceVariable)
        {
            switch (serviceVariable.VariableType)
            {
                case ServiceVariableType.InterviewRandom:
                    return interview.InterviewId.GetRandomDouble().ToString(CultureInfo.InvariantCulture);
                case ServiceVariableType.InterviewKey:
                    return interview.InterviewKey ?? string.Empty;
            }

            return String.Empty;
        }
    }
}
