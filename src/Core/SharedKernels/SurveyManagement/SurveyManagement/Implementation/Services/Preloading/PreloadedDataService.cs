﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class PreloadedDataService : IPreloadedDataService
    {
        private readonly QuestionnaireExportStructure exportStructure;
        private readonly QuestionnaireRosterStructure questionnaireRosterStructure;
        private readonly QuestionnaireDocument questionnaireDocument;
        private readonly IDataFileService dataFileService;
        private readonly IQuestionDataParser dataParser;

        public PreloadedDataService(QuestionnaireExportStructure exportStructure, QuestionnaireRosterStructure questionnaireRosterStructure,
            QuestionnaireDocument questionnaireDocument, IDataFileService dataFileService, IQuestionDataParser dataParser)
        {
            this.exportStructure = exportStructure;
            this.questionnaireRosterStructure = questionnaireRosterStructure;
            this.questionnaireDocument = questionnaireDocument;
            this.dataFileService = dataFileService;
            this.dataParser = dataParser;
        }

        public HeaderStructureForLevel FindLevelInPreloadedData(string levelFileName)
        {
            return
                exportStructure.HeaderToLevelMap.Values.FirstOrDefault(
                    header => levelFileName.IndexOf(dataFileService.CreateValidFileName(header.LevelName), StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public PreloadedDataByFile GetParentDataFile(string levelFileName, PreloadedDataByFile[] allLevels)
        {
            var levelExportStructure = FindLevelInPreloadedData(levelFileName);
            if (levelExportStructure == null)
                return null;

            if (!questionnaireRosterStructure.RosterScopes.ContainsKey(levelExportStructure.LevelId))
                return null;

            var rosterScopeDescription = questionnaireRosterStructure.RosterScopes[levelExportStructure.LevelId];
            var scopeVector = rosterScopeDescription.RosterIdToRosterVectorMap.Values.FirstOrDefault();

            var parentLevelId = scopeVector == null || scopeVector.Length == 0
                ? questionnaireRosterStructure.QuestionnaireId
                : scopeVector.Last();

            var parentLevel = exportStructure.HeaderToLevelMap.Values.FirstOrDefault(l => l.LevelId == parentLevelId);
            if (parentLevel == null)
                return null;

            return GetDataFileByLevelName(allLevels, parentLevel.LevelName);
        }

        public decimal[] GetAvalibleIdListForParent(PreloadedDataByFile parentDataFile, Guid levelId, string parentIdValue)
        {
            var idIndexInParentDataFile = this.GetIdColumnIndex(parentDataFile);

            var row = parentDataFile.Content.FirstOrDefault(record => record[idIndexInParentDataFile] == parentIdValue);
            if (row == null)
                return null;

            if (!questionnaireRosterStructure.RosterScopes.ContainsKey(levelId))
                return null;

            var rosterScopeDescription = questionnaireRosterStructure.RosterScopes[levelId];

            if (rosterScopeDescription.ScopeType == RosterScopeType.Fixed)
            {
                return
                    questionnaireDocument.FirstOrDefault<IGroup>(g => g.PublicKey == levelId)
                        .RosterFixedTitles.Select((t, i) => (decimal) i)
                        .ToArray();
            }

            var rosterSizeQuestion = questionnaireDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == levelId);

            var levelExportStructure = FindLevelInPreloadedData(parentDataFile.FileName);
            if (levelExportStructure == null)
                return null;

            var answerOnRosterSizeQuestion = BuildAnswerByVariableName(levelExportStructure, rosterSizeQuestion.StataExportCaption,
                parentDataFile.Header, row);

            if (!answerOnRosterSizeQuestion.HasValue)
                return null;

            var answerObject = answerOnRosterSizeQuestion.Value.Value;
            if (rosterScopeDescription.ScopeType == RosterScopeType.Numeric)
            {
                var intValueOfNumericQuestion = (int) answerObject;
                return Enumerable.Range(0, intValueOfNumericQuestion).Select(i => (decimal) i).ToArray();
            }

            if (rosterScopeDescription.ScopeType == RosterScopeType.MultyOption)
            {
                return (decimal[]) answerObject;
            }

            if (rosterScopeDescription.ScopeType == RosterScopeType.TextList)
            {
                return ((Tuple<decimal, string>[]) answerObject).Select(a => a.Item1).ToArray();
            }
            return null;
        }

        public KeyValuePair<Guid, object>? ParseQuestion(string answer, string variableName)
        {
            return dataParser.Parse(answer, variableName, questionnaireDocument);
        }

        public decimal GetRecordIdValueAsDecimal(string[] dataFileRecord, int idColumnIndex)
        {
            var cellValue = dataFileRecord[idColumnIndex];
            return decimal.Parse(cellValue);
        }

        public int GetIdColumnIndex(PreloadedDataByFile dataFile)
        {
            return dataFile.Header.ToList().FindIndex(header => header == "Id");
        }

        public int GetParentIdColumnIndex(PreloadedDataByFile dataFile)
        {
            return dataFile.Header.ToList().FindIndex(header => header == "ParentId");
        }

        public PreloadedDataDto[] CreatePreloadedDataDto(PreloadedDataByFile[] allLevels)
        {
            var topLevelData = GetDataFileByLevelName(allLevels, questionnaireDocument.Title);

            if (topLevelData == null)
                return null;

            var idColumnIndex = GetIdColumnIndex(topLevelData);

            var result = new List<PreloadedDataDto>();
            
            foreach (var topLevelRow in topLevelData.Content)
            {
                var rowId = topLevelRow[idColumnIndex];
                var answersToFeaturedQuestions = BuildAnswerForLevel(topLevelRow, topLevelData.Header, topLevelData.FileName);

                var rosterAnswers = this.GetAnswers(topLevelData.FileName, rowId, new decimal[0], allLevels);
                var levels = new List<PreloadedLevelDto>() { new PreloadedLevelDto(new decimal[0], answersToFeaturedQuestions) };
                levels.AddRange(rosterAnswers);

                result.Add(new PreloadedDataDto(rowId, levels.ToArray()));
            }
            return result.ToArray();
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
            return allLevels.FirstOrDefault(l => l.FileName.IndexOf(dataFileService.CreateValidFileName(name), StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private PreloadedLevelDto[] GetAnswers(string levelName, string parentId, decimal[] rosterVector, PreloadedDataByFile[] rosterData)
        {
            var result = new List<PreloadedLevelDto>();
            var childFiles = GetChildDataFiles(levelName, rosterData);

            foreach (var preloadedDataByFile in childFiles)
            {
                var parentIdColumnIndex = GetParentIdColumnIndex(preloadedDataByFile);
                var idColumnIndex = GetIdColumnIndex(preloadedDataByFile);
                var childRecrordsOfCurrentRow =
                    preloadedDataByFile.Content.Where(
                        record => record[parentIdColumnIndex] == parentId).ToArray();

                foreach (var rosterRow in childRecrordsOfCurrentRow)
                {
                    var newRosterVetor = new decimal[rosterVector.Length + 1];
                    rosterVector.CopyTo(newRosterVetor, 0);
                    newRosterVetor[newRosterVetor.Length - 1] = GetRecordIdValueAsDecimal(rosterRow, idColumnIndex);

                    var rosterAnswers = BuildAnswerForLevel(rosterRow, preloadedDataByFile.Header, preloadedDataByFile.FileName);

                    result.Add(new PreloadedLevelDto(newRosterVetor, rosterAnswers));

                    result.AddRange(this.GetAnswers(preloadedDataByFile.FileName, rosterRow[idColumnIndex], newRosterVetor, rosterData));
                }
            }

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

            return dataParser.BuildAnswerFromStringArray(row.Where((v, i) => headerIndexes.Contains(i)).ToArray(),
                exportedHeaderItem.VariableName, questionnaireDocument);
        }

        private PreloadedDataByFile[] GetChildDataFiles(string levelFileName, PreloadedDataByFile[] allLevels)
        {
            var levelExportStructure = FindLevelInPreloadedData(levelFileName);
            if (levelExportStructure == null)
                return new PreloadedDataByFile[0];
            IEnumerable<RosterScopeDescription> children = Enumerable.Empty<RosterScopeDescription>();
            if (levelExportStructure.LevelId == questionnaireRosterStructure.QuestionnaireId)
            {
                children =
                    questionnaireRosterStructure.RosterScopes.Values.Where(
                        scope => scope.RosterIdToRosterVectorMap.Values.Any(r => r.Length == 0));
            }
            else
                children =
                    questionnaireRosterStructure.RosterScopes.Values.Where(
                        scope =>
                            scope.RosterIdToRosterVectorMap.Values.Any(r => r.LastOrDefault() == levelExportStructure.LevelId) &&
                                exportStructure.HeaderToLevelMap.ContainsKey(levelExportStructure.LevelId));

            return
                children
                    .Select(scope => exportStructure.HeaderToLevelMap[scope.ScopeId]).Select(
                        child =>
                            GetDataFileByLevelName(allLevels, child.LevelName)).Where(file => file != null).ToArray();
        }
    }
}
