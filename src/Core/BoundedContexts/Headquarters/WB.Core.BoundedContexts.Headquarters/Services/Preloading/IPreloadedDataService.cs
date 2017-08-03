using System;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    internal interface IPreloadedDataService
    {
        HeaderStructureForLevel FindLevelInPreloadedData(string levelFileName);

        PreloadedDataByFile GetTopLevelData(PreloadedDataByFile[] allLevels);

        PreloadedDataByFile GetParentDataFile(string levelFileName, PreloadedDataByFile[] allLevels);
        
        int GetColumnIndexByHeaderName(PreloadedDataByFile dataFile, string columnName);

        int GetIdColumnIndex(PreloadedDataByFile dataFile);

        int[] GetParentIdColumnIndexes(PreloadedDataByFile dataFile);

        int[] GetAvailableIdListForParent(PreloadedDataByFile parentDataFile, ValueVector<Guid> levelScopeVector, string[] parentIdValues, PreloadedDataByFile[] allLevels);

        ValueParsingResult ParseQuestionInLevel(string answer, string columnName, HeaderStructureForLevel level, out object parsedValue);

        AssignmentPreloadedDataRecord[] CreatePreloadedDataDtosFromPanelData(PreloadedDataByFile[] allLevels);

        AssignmentPreloadedDataRecord[] CreatePreloadedDataDtoFromAssignmentData(PreloadedDataByFile sampleDataFile);

        string GetValidFileNameForTopLevelQuestionnaire();

        bool IsQuestionRosterSize(string variableName);

        bool IsRosterSizeQuestionForLongRoster(Guid questionId);
    }
}
