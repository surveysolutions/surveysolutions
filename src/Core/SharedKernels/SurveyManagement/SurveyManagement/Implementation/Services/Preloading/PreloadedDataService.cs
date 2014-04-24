using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.FileSystem;
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

        public PreloadedDataService(QuestionnaireExportStructure exportStructure, QuestionnaireRosterStructure questionnaireRosterStructure,
            QuestionnaireDocument questionnaireDocument, IDataFileService dataFileService)
        {
            this.exportStructure = exportStructure;
            this.questionnaireRosterStructure = questionnaireRosterStructure;
            this.questionnaireDocument = questionnaireDocument;
            this.dataFileService = dataFileService;
        }

        public HeaderStructureForLevel FindLevelInPreloadedData(string levelFileName)
        {
            return
                exportStructure.HeaderToLevelMap.Values.FirstOrDefault(
                    header => levelFileName.IndexOf(dataFileService.CreateValidFileName(header.LevelName), StringComparison.OrdinalIgnoreCase) >= 0);
        }


        public PreloadedDataByFile GetDataFileByLevelName(PreloadedDataByFile[] allLevels, string name)
        {
            return allLevels.FirstOrDefault(l => l.FileName.IndexOf(dataFileService.CreateValidFileName(name), StringComparison.OrdinalIgnoreCase) >= 0);
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
            var parentIdIndex = this.GetIdColumnIndex(parentDataFile);
            var row = parentDataFile.Content.FirstOrDefault(record => record[parentIdIndex] == parentIdValue);
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
            if (rosterScopeDescription.ScopeType == RosterScopeType.Numeric)
            {
                var indexOfNumericRosterSizeQuestion = parentDataFile.Header.ToList().FindIndex(header => header == rosterSizeQuestion.StataExportCaption);
                var valueOfNumericQuestion = row[indexOfNumericRosterSizeQuestion];
                int intValueOfNumericQuestion;
                if (!int.TryParse(valueOfNumericQuestion, out  intValueOfNumericQuestion))
                    return null;
                return Enumerable.Range(0, intValueOfNumericQuestion).Select(i => (decimal)i).ToArray();
            }

        /*    var parentExportStructure = exportStructure.HeaderToLevelMap[FindLevelInPreloadedData(parentDataFile.FileName).LevelId];
            var rosterSizeHeaderItem =
                parentExportStructure.HeaderItems.Values.FirstOrDefault(h => h.VariableName == rosterSizeQuestion.StataExportCaption);
            
            if (rosterSizeHeaderItem == null)
                return new decimal[0];

            var indexesOfNumericRosterSizeQuestion = new List<int>();
            for (int i = 0; i < parentDataFile.Header.Length; i++)
            {
                if (rosterSizeHeaderItem.ColumnNames.Contains(parentDataFile.Header[i]))
                    indexesOfNumericRosterSizeQuestion.Add(i);
            }

            if (rosterScopeDescription.ScopeType == RosterScopeType.TextList)
            {
                
            }*/

            return null;
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

        public PreloadedDataByFile[] GetChildDataFiles(string levelFileName, PreloadedDataByFile[] allLevels)
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
    }
}
