using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class PreloadedDataService : IPreloadedDataService
    {
        private readonly QuestionnaireExportStructure exportStructure;
        private readonly QuestionnaireRosterStructure questionnaireRosterStructure;
        private readonly QuestionnaireDocument questionnaireDocument;
        private readonly IQuestionDataParser dataParser;
        

        private const string IdColumnName = "Id";
        private const string ParentIdColumnName = "ParentId";
        private const string SupervisorNameColumnName = "_Supervisor";

        public PreloadedDataService(QuestionnaireExportStructure exportStructure, QuestionnaireRosterStructure questionnaireRosterStructure,
            QuestionnaireDocument questionnaireDocument, IQuestionDataParser dataParser)
        {
            this.exportStructure = exportStructure;
            this.questionnaireRosterStructure = questionnaireRosterStructure;
            this.questionnaireDocument = questionnaireDocument;
            this.dataParser = dataParser;
        }
        
        public HeaderStructureForLevel FindLevelInPreloadedData(string levelFileName)
        {
            var levelNameWithoutExtension = Path.GetFileNameWithoutExtension(levelFileName);
            return
                exportStructure.HeaderToLevelMap.Values.FirstOrDefault(
                    header => string.Equals(levelNameWithoutExtension, header.LevelName, StringComparison.OrdinalIgnoreCase));
        }

        public PreloadedDataByFile GetParentDataFile(string levelFileName, PreloadedDataByFile[] allLevels)
        {
            var levelExportStructure = FindLevelInPreloadedData(levelFileName);
            if (levelExportStructure == null)
                return null;

            if (!questionnaireRosterStructure.RosterScopes.ContainsKey(levelExportStructure.LevelScopeVector))
                return null;

            var rosterScopeDescription = questionnaireRosterStructure.RosterScopes[levelExportStructure.LevelScopeVector];

            var parentLevel =
                exportStructure.HeaderToLevelMap.Values.FirstOrDefault(
                    l =>
                        l.LevelScopeVector.Length == rosterScopeDescription.ScopeVector.Length - 1 &&
                          rosterScopeDescription.ScopeVector.Take(l.LevelScopeVector.Length).SequenceEqual(l.LevelScopeVector));
            if (parentLevel == null)
                return null;

            return GetDataFileByLevelName(allLevels, parentLevel.LevelName);
        }

        public decimal[] GetAvailableIdListForParent(PreloadedDataByFile parentDataFile, ValueVector<Guid> levelScopeVector, string[] parentIdValues)
        {
            if (parentIdValues == null || parentIdValues.Length == 0)
                return null;

            var idIndexInParentDataFile = this.GetIdColumnIndex(parentDataFile);
            var parentIdColumnIndexesOfParentDataFile = this.GetParentIdColumnIndexes(parentDataFile)?? new int[0];
            var row =
                parentDataFile.Content.FirstOrDefault(
                    record =>
                        record[idIndexInParentDataFile] == parentIdValues.First() &&
                            parentIdColumnIndexesOfParentDataFile.Select(x => record[x]).SequenceEqual(parentIdValues.Skip(1)));
            if (row == null)
                return null;

            if (!questionnaireRosterStructure.RosterScopes.ContainsKey(levelScopeVector))
                return null;

            var rosterScopeDescription = questionnaireRosterStructure.RosterScopes[levelScopeVector];

            if (rosterScopeDescription.ScopeType == RosterScopeType.Fixed)
            {
                return
                    questionnaireDocument.FirstOrDefault<IGroup>(g => g.PublicKey == levelScopeVector.Last())
                        .FixedRosterTitles.Select(x => x.Value).ToArray();
            }

            var rosterSizeQuestion = questionnaireDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == levelScopeVector.Last());

            var levelExportStructure = FindLevelInPreloadedData(parentDataFile.FileName);
            if (levelExportStructure == null)
                return null;

            var answerOnRosterSizeQuestion = BuildAnswerByVariableName(levelExportStructure, rosterSizeQuestion.StataExportCaption,
                parentDataFile.Header, row);

            if (!answerOnRosterSizeQuestion.HasValue)
                return new decimal[0];

            var answerObject = answerOnRosterSizeQuestion.Value.Value;

            if (rosterScopeDescription.ScopeType == RosterScopeType.Numeric)
            {
                return Enumerable.Range(0, (int)answerObject).Select(i => (decimal)i).ToArray();
            }

            if (rosterScopeDescription.ScopeType == RosterScopeType.MultyOption)
            {
                return answerObject as decimal[];
            }

            if (rosterScopeDescription.ScopeType == RosterScopeType.TextList)
            {
                return ((Tuple<decimal, string>[])answerObject).Select(a => a.Item1).ToArray();
            }
            return null;
        }

        public ValueParsingResult ParseQuestion(string answer, IQuestion question, out KeyValuePair<Guid, object> parsedValue)
        {
            return dataParser.TryParse(answer, question, questionnaireDocument, out parsedValue);
        }

        public int GetIdColumnIndex(PreloadedDataByFile dataFile)
        {
            return GetColumnIndexByHeaderName(dataFile, IdColumnName);
        }

        public int GetColumnIndexByHeaderName(PreloadedDataByFile dataFile, string columnName)
        {
            return dataFile.Header.ToList().FindIndex(header => string.Equals(header, columnName, StringComparison.InvariantCultureIgnoreCase));
        }

        public int[] GetParentIdColumnIndexes(PreloadedDataByFile dataFile)
        {
            var levelExportStructure = this.FindLevelInPreloadedData(dataFile.FileName);
            if (levelExportStructure == null || levelExportStructure.LevelScopeVector == null ||
                levelExportStructure.LevelScopeVector.Length == 0)
                return null;

            var columnIndexOfParentIdindexMap = new Dictionary<int,int>();
            var listOfAvailableParentIdIndexes = levelExportStructure.LevelScopeVector.Select((l, i) => i + 1).ToArray();

            for (int i = 0; i < dataFile.Header.Length; i++)
            {
                var columnName = dataFile.Header[i];
                if (!columnName.StartsWith(ParentIdColumnName, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var parentNumberString = columnName.Substring(ParentIdColumnName.Length);
                int parentNumber;
                if (int.TryParse(parentNumberString, out parentNumber))
                {
                    if (listOfAvailableParentIdIndexes.Contains(parentNumber))
                        columnIndexOfParentIdindexMap.Add(i, parentNumber);
                }
            }
            if (columnIndexOfParentIdindexMap.Values.Distinct().Count() != levelExportStructure.LevelScopeVector.Length)
                return null;
            return columnIndexOfParentIdindexMap.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
        }

        public PreloadedDataRecord[] CreatePreloadedDataDtosFromPanelData(PreloadedDataByFile[] allLevels)
        {
            var topLevelExportData = exportStructure.HeaderToLevelMap.Values.FirstOrDefault(l => l.LevelScopeVector.Length == 0);
            if (topLevelExportData == null)
                return null;

            var topLevelData = GetDataFileByLevelName(allLevels, topLevelExportData.LevelName);

            if (topLevelData == null)
                return null;

            var idColumnIndex = GetIdColumnIndex(topLevelData);
            var supervisorNameIndex = GetSupervisorNameIndex(topLevelData);
            var result = new List<PreloadedDataRecord>();
            
            foreach (var topLevelRow in topLevelData.Content)
            {
                var rowId = topLevelRow[idColumnIndex];
                var answersByTopLevel = BuildAnswerForLevel(topLevelRow, topLevelData.Header, topLevelData.FileName);
                var levels = new List<PreloadedLevelDto>() { new PreloadedLevelDto(new decimal[0], answersByTopLevel) };
                var answersinsideRosters = this.GetHierarchicalAnswersByLevelName(topLevelData.FileName, new[] { rowId }, allLevels);
                levels.AddRange(answersinsideRosters);

                result.Add(new PreloadedDataRecord
                {
                    PreloadedDataDto = new PreloadedDataDto(rowId, levels.ToArray()),
                    SupervisorName = CheckAndGetSupervisorNameForLevel(topLevelRow, supervisorNameIndex)
                });
            }
            return result.ToArray();
        }

        public PreloadedDataRecord[] CreatePreloadedDataDtoFromSampleData(PreloadedDataByFile sampleDataFile)
        {
            var result = new List<PreloadedDataRecord>();

            var supervisorNameIndex = GetSupervisorNameIndex(sampleDataFile);
            foreach (var contentRow in sampleDataFile.Content)
            {
                var answersToFeaturedQuestions = BuildAnswerForLevel(contentRow, sampleDataFile.Header,
                    GetValidFileNameForTopLevelQuestionnaire());
                
                result.Add(
                    new PreloadedDataRecord
                    {
                        PreloadedDataDto = new PreloadedDataDto(Guid.NewGuid().FormatGuid(), new[] { new PreloadedLevelDto(new decimal[0], answersToFeaturedQuestions)}),
                        SupervisorName = CheckAndGetSupervisorNameForLevel(contentRow, supervisorNameIndex)
                    });
            }
            return result.ToArray();
        }

        private int GetSupervisorNameIndex(PreloadedDataByFile dataFile)
        {
            return dataFile.Header.ToList().FindIndex(header => string.Equals(header, SupervisorNameColumnName, StringComparison.InvariantCultureIgnoreCase));
        }

        private string CheckAndGetSupervisorNameForLevel(string[] row, int supervisorNameIndex)
        {
            return (supervisorNameIndex >= 0) ? row[supervisorNameIndex] : string.Empty;
        }

        public string GetValidFileNameForTopLevelQuestionnaire()
        {
            return exportStructure.HeaderToLevelMap.Values.FirstOrDefault(level => level.LevelScopeVector.Count == 0).LevelName;
        }

        public Dictionary<string, int[]> GetColumnIndexesGoupedByQuestionVariableName(PreloadedDataByFile parentDataFile)
        {
            var levelExportStructure = FindLevelInPreloadedData(parentDataFile.FileName);
            if (levelExportStructure == null)
                return null;
            var presentQuestions = new Dictionary<string, int[]>();
            foreach (var exportedHeaderItem in levelExportStructure.HeaderItems.Values)
            {
                var headerIndexes = new List<int>();
                for (int i = 0; i < parentDataFile.Header.Length; i++)
                {
                    if (exportedHeaderItem.ColumnNames.Contains(parentDataFile.Header[i]))
                        headerIndexes.Add(i);
                }
                if (!headerIndexes.Any())
                    continue;

                presentQuestions.Add(exportedHeaderItem.VariableName, headerIndexes.ToArray());
            }
            return presentQuestions;
        }

        private PreloadedDataByFile GetDataFileByLevelName(PreloadedDataByFile[] allLevels, string name)
        {
            return allLevels.FirstOrDefault(l => string.Equals(Path.GetFileNameWithoutExtension(l.FileName),name, StringComparison.OrdinalIgnoreCase));
        }

        private PreloadedLevelDto[] GetHierarchicalAnswersByLevelName(string levelName, string[] parentIds, PreloadedDataByFile[] rosterData)
        {
            var result = new List<PreloadedLevelDto>();
            var childFiles = GetChildDataFiles(levelName, rosterData);

            foreach (var preloadedDataByFile in childFiles)
            {
                var parentIdColumnIndexes = GetParentIdColumnIndexes(preloadedDataByFile);
                var idColumnIndex = this.GetIdColumnIndex(preloadedDataByFile);
                var childRecordsOfCurrentRow =
                    preloadedDataByFile.Content.Where(
                        record => parentIdColumnIndexes.Select(parentIdColumnIndex => record[parentIdColumnIndex]).SequenceEqual(parentIds))
                        .ToArray();

                foreach (var rosterRow in childRecordsOfCurrentRow)
                {
                    var rosterAnswers = BuildAnswerForLevel(rosterRow, preloadedDataByFile.Header, preloadedDataByFile.FileName);

                    var newParentIds = this.ExtendParentIdsVectorWithCurrentRecordIdOnAFirstPlace(parentIds, rosterRow[idColumnIndex]);
                    var newRosterVetor = this.CreateRosterVectorFromParentIds(newParentIds);
                 
                    result.Add(new PreloadedLevelDto(newRosterVetor, rosterAnswers));
                    result.AddRange(this.GetHierarchicalAnswersByLevelName(preloadedDataByFile.FileName, newParentIds, rosterData));
                }
            }

            return result.ToArray();
        }

        private string[] ExtendParentIdsVectorWithCurrentRecordIdOnAFirstPlace(string[] parentIds, string currentRecordId)
        {
            var newParentIds = new string[parentIds.Length + 1];
            newParentIds[0] = currentRecordId;
            for (int i = 0; i < parentIds.Length; i++)
            {
                newParentIds[i + 1] = parentIds[i];
            }
            return newParentIds;
        }

        private decimal[] CreateRosterVectorFromParentIds(string[] parentIds)
        {
            var result = new List<decimal>();
            for (int i = 0; i < parentIds.Length-1; i++)
            {
                result.Add(decimal.Parse(parentIds[i]));
            }
            result.Reverse();
            return result.ToArray();
        }

        private Dictionary<Guid, object> BuildAnswerForLevel(string[] row, string[] header, string levelFileName)
        {
            var levelExportStructure = FindLevelInPreloadedData(levelFileName);
            if (levelExportStructure == null)
                return null;
            var result = new Dictionary<Guid, object>();

            foreach (var exportedHeaderItem in levelExportStructure.HeaderItems.Values)
            {
                var parsedAnswer = BuildAnswerByVariableName(levelExportStructure, exportedHeaderItem.VariableName, header, row);

                if (parsedAnswer.HasValue)
                    result.Add(parsedAnswer.Value.Key, parsedAnswer.Value.Value);
            }
            return result;
        }

        private KeyValuePair<Guid,object>? BuildAnswerByVariableName(HeaderStructureForLevel levelExportStructure, string variableName, string[] header, string[] row)
        {
            var exportedHeaderItem =
                levelExportStructure.HeaderItems.Values.FirstOrDefault(
                    h => string.Equals(h.VariableName, variableName, StringComparison.OrdinalIgnoreCase));
            if (exportedHeaderItem == null)
                return null;

            var headerIndexes = new List<int>();

            for (int i = 0; i < header.Length; i++)
            {
                if (exportedHeaderItem.ColumnNames.Contains(header[i]))
                    headerIndexes.Add(i);
            }
            if (!headerIndexes.Any())
                return null;

            var question = this.GetQuestionByVariableName(exportedHeaderItem.VariableName);

            return dataParser.BuildAnswerFromStringArray(row.Where((v, i) => headerIndexes.Contains(i)).ToArray(), question, questionnaireDocument);
        }

        private PreloadedDataByFile[] GetChildDataFiles(string levelFileName, PreloadedDataByFile[] allLevels)
        {
            var levelExportStructure = FindLevelInPreloadedData(levelFileName);
            if (levelExportStructure == null)
                return new PreloadedDataByFile[0];
            IEnumerable<RosterScopeDescription> children = Enumerable.Empty<RosterScopeDescription>();
            if (levelExportStructure.LevelScopeVector.Length == 0)
            {
                children =
                    questionnaireRosterStructure.RosterScopes.Values.Where(
                        scope => scope.ScopeVector.Length == 1);
            }
            else
                children =
                    questionnaireRosterStructure.RosterScopes.Values.Where(
                        scope =>
                            scope.ScopeVector.Length == levelExportStructure.LevelScopeVector.Length + 1 &&
                                scope.ScopeVector.Take(levelExportStructure.LevelScopeVector.Length)
                                    .SequenceEqual(levelExportStructure.LevelScopeVector) &&
                                exportStructure.HeaderToLevelMap.ContainsKey(levelExportStructure.LevelScopeVector));

            return
                children
                    .Select(scope => exportStructure.HeaderToLevelMap[scope.ScopeVector]).Select(
                        child =>
                            GetDataFileByLevelName(allLevels, child.LevelName)).Where(file => file != null).ToArray();
        }

        public IQuestion GetQuestionByVariableName(string variableName)
        {
            return questionnaireDocument.FirstOrDefault<IQuestion>(q => q.StataExportCaption.Equals(variableName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
