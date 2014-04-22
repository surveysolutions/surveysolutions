using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class PreloadedDataService : IPreloadedDataService
    {
        private readonly QuestionnaireExportStructure exportStructure;
        private readonly QuestionnaireRosterStructure questionnaireRosterStructure;
        
        public PreloadedDataService(QuestionnaireExportStructure exportStructure, QuestionnaireRosterStructure questionnaireRosterStructure)
        {
            this.exportStructure = exportStructure;
            this.questionnaireRosterStructure = questionnaireRosterStructure;
        }

        public HeaderStructureForLevel FindLevelInPreloadedData(string levelFileName)
        {
            return
                exportStructure.HeaderToLevelMap.Values.FirstOrDefault(
                    header => levelFileName.IndexOf(header.LevelName, StringComparison.OrdinalIgnoreCase) >= 0);
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
                            GetDataFileByLevelName(allLevels, child.LevelName)).ToArray();
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

        private PreloadedDataByFile GetDataFileByLevelName(PreloadedDataByFile[] allLevels, string name)
        {
            return allLevels.FirstOrDefault(l => l.FileName.Contains(name));
        }
    }
}
