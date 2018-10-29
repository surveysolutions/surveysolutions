using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Utils;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.CsvExport.Exporters
{
    [Localizable(false)]
    public class InterviewsExporter : IInterviewsExporter
    {
        private readonly IInterviewFactory interviewFactory;
        private readonly string dataFileExtension = "tab";

        private readonly ILogger<InterviewsExporter> logger;
        private readonly ICsvWriter csvWriter;
        private readonly IOptions<InterviewDataExportSettings> options;
        private readonly IInterviewErrorsExporter errorsExporter;
        private readonly IExportQuestionService exportQuestionService;

        public InterviewsExporter(
            IExportQuestionService exportQuestionService,
            IInterviewFactory interviewFactory,
            IInterviewErrorsExporter errorsExporter,
            ICsvWriter csvWriter,
            IOptions<InterviewDataExportSettings> options,
            ILogger<InterviewsExporter> logger)
        {
            this.exportQuestionService = exportQuestionService;
            this.interviewFactory = interviewFactory ?? throw new ArgumentNullException(nameof(interviewFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
            this.options = options;
            this.errorsExporter = errorsExporter ?? throw new ArgumentNullException(nameof(errorsExporter));
        }

        public async Task ExportAsync(TenantInfo tenant,
            QuestionnaireExportStructure questionnaireExportStructure,
            QuestionnaireDocument questionnaire,
            List<InterviewToExport> interviewsToExport,
            string basePath,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            await this.DoExportAsync(tenant, questionnaireExportStructure, questionnaire, basePath, interviewsToExport, progress, cancellationToken);
            stopwatch.Stop();
            this.logger.Log(LogLevel.Information, $"Export of {interviewsToExport.Count:N0} interview datas for questionnaire" +
                                                  $" {questionnaireExportStructure.QuestionnaireId} finised. Took {stopwatch.Elapsed:c} to complete");
        }

        private Task DoExportAsync(TenantInfo tenant,
            QuestionnaireExportStructure questionnaireExportStructure,
            QuestionnaireDocument questionnaire,
            string basePath,
            List<InterviewToExport> interviewIdsToExport,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
            return this.ExportInterviewsAsync(tenant, interviewIdsToExport, basePath, questionnaireExportStructure, questionnaire, progress, cancellationToken);
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

        private async Task ExportInterviewsAsync(TenantInfo tenant, List<InterviewToExport> interviewIdsToExport,
            string basePath,
            QuestionnaireExportStructure questionnaireExportStructure,
            QuestionnaireDocument questionnaire,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            long totalInterviewsProcessed = 0;
            
            var batchOptions = new BatchOptions { Max = this.options.Value.MaxRecordsCountPerOneExportQuery };

            foreach (var batch in interviewIdsToExport.BatchInTime(batchOptions, logger))
            {
                var exportBulk = new List<InterviewExportedDataRecord>();
                cancellationToken.ThrowIfCancellationRequested();
                var interviewIds = batch.Select(b => b.Id).ToArray();
                var trace = Stopwatch.StartNew();

                var interviewEntities = await this.interviewFactory.GetInterviewEntities(tenant, interviewIds);
                var interviewEntitiesLookup = interviewEntities.ToLookup(ie => ie.InterviewId);

                logger.LogTrace($"Took {trace.ElapsedMilliseconds}ms to query {batch.Count} interviews {interviewEntities.Count} rows");
                trace.Restart();
                
                Parallel.ForEach(batch, interviewToExport =>
                {
                    var interviewExportedDataRecord = this.ExportSingleInterview(interviewToExport,
                        interviewEntitiesLookup[interviewToExport.Id].ToList(),
                        questionnaireExportStructure,
                        questionnaire,
                        basePath);

                    lock (exportBulk)
                    {
                        exportBulk.Add(interviewExportedDataRecord);
                    }

                    var interviewsProcessed = Interlocked.Increment(ref totalInterviewsProcessed);
                    progress.Report(interviewsProcessed.PercentOf(interviewIdsToExport.Count));
                });

                logger.LogTrace($"Took {trace.ElapsedMilliseconds}ms to process {batch.Count} interviews {interviewEntities.Count} rows");
                trace.Restart();

                this.WriteInterviewDataToCsvFile(basePath, exportBulk);
                logger.LogTrace($"Took {trace.ElapsedMilliseconds}ms to write {batch.Count} interviews {interviewEntities.Count} rows");
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

        private readonly Stopwatch exportProcessingStopwatch = new Stopwatch();

        private InterviewExportedDataRecord ExportSingleInterview(InterviewToExport interviewToExport,
            List<InterviewEntity> interview, QuestionnaireExportStructure exportStructure, QuestionnaireDocument questionnaire, string basePath)
        {
            exportProcessingStopwatch.Start();
            List<string[]> errors = errorsExporter.Export(exportStructure, questionnaire, interview, basePath, interviewToExport.Key);

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

        public InterviewDataExportView CreateInterviewDataExportView(QuestionnaireExportStructure exportStructure,
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
                    throw new ArgumentException("Unknown export header");
                }
            }
            return result.ToArray();
        }

        private string GetTextValueForTextListQuestion(InterviewData interview, RosterVector rosterVector, Guid id)
        {
            decimal itemToSearch = rosterVector.Last();

            for (var i = 1; i <= rosterVector.Length; i++)
            {
                var levelForVector =
                    interview.Levels.GetOrNull(
                        this.CreateLevelIdFromPropagationVector(rosterVector.Take(rosterVector.Length - i)));

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

        private string CreateLevelIdFromPropagationVector(RosterVector vector)
        {
            return vector.Length == 0 ? "#" : vector.ToString();
        }

        private List<InterviewLevel> GetLevelsFromInterview(InterviewData interview, ValueVector<Guid> levelVector)
        {
            if (!levelVector.Any())
            {
                var levels = interview.Levels.Values.Where(level => level.RosterScope.Equals(new ValueVector<Guid>())).ToList();
                return levels.Any()
                    ? levels
                    : new List<InterviewLevel> { new InterviewLevel { RosterVector = RosterVector.Empty } };
            }
            return interview.Levels.Values.Where(level => level.RosterScope.Equals(levelVector)).ToList();
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

                        void InsertOrSetAt(ServiceVariableType type, string value)
                        {
                            var index = ServiceColumns.SystemVariables[type].Index;

                            if (systemVariableValues.Count < index + 1)
                            {
                                systemVariableValues.Insert(index, value);
                            }
                            else
                            {
                                systemVariableValues[index] = value;
                            }
                        }

                        InsertOrSetAt(ServiceVariableType.HasAnyError, interviewId.ErrorsCount.ToString());
                        InsertOrSetAt(ServiceVariableType.InterviewStatus, interviewId.Status.ToString());
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
