using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Preloading
{
    internal interface IPreloadedDataService
    {
        HeaderStructureForLevel FindLevelInPreloadedData(string levelFileName);
        PreloadedDataByFile GetParentDataFile(string levelFileName, PreloadedDataByFile[] allLevels);
        
        int GetIdColumnIndex(PreloadedDataByFile dataFile);
        int[] GetParentIdColumnIndexes(PreloadedDataByFile dataFile);
        decimal[] GetAvailableIdListForParent(PreloadedDataByFile parentDataFile, ValueVector<Guid> levelScopeVector, string[] parentIdValues);
        
        Dictionary<string, int[]> GetColumnIndexesGoupedByQuestionVariableName(PreloadedDataByFile parentDataFile);
        ValueParsingResult ParseQuestion(string answer, IQuestion question, out KeyValuePair<Guid, object> parsedValue);

        PreloadedDataDto[] CreatePreloadedDataDtosFromPanelData(PreloadedDataByFile[] allLevels);
        PreloadedDataDto[] CreatePreloadedDataDtoFromSampleData(PreloadedDataByFile sampleDataFile);
        string GetValidFileNameForTopLevelQuestionnaire();

        IQuestion GetQuestionByVariableName(string variableName);

    }
}
