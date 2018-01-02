using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    internal interface IPreloadedDataService
    {
        HeaderStructureForLevel FindLevelInPreloadedData(string levelFileName);

        PreloadedDataByFile GetTopLevelData(PreloadedData allLevels);

        PreloadedDataByFile GetParentDataFile(string levelFileName, PreloadedData allLevels);
        
        int GetColumnIndexByHeaderName(PreloadedDataByFile dataFile, string columnName);

        int GetIdColumnIndex(PreloadedDataByFile dataFile);

        int[] GetParentIdColumnIndexes(PreloadedDataByFile dataFile);

        int[] GetAvailableIdListForParent(PreloadedDataByFile parentDataFile, ValueVector<Guid> levelScopeVector, string[] parentIdValues, PreloadedData allLevels);

        ValueParsingResult ParseQuestionInLevel(string answer, string columnName, HeaderStructureForLevel level, out object parsedValue);

        AssignmentPreloadedDataRecord[] CreatePreloadedDataDtosFromPanelData(PreloadedData allLevels);

        AssignmentPreloadedDataRecord[] CreatePreloadedDataDtoFromAssignmentData(PreloadedDataByFile sampleDataFile);

        string GetValidFileNameForTopLevelQuestionnaire();

        bool IsQuestionRosterSize(string variableName);

        bool IsRosterSizeQuestionForLongRoster(Guid questionId);
        IEnumerable<string> GetAllParentColumnNamesForLevel(ValueVector<Guid> levelScopeVector);
        bool IsVariableColumn(string columnName);
    }
}
