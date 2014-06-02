using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
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
        ValueParsingResult ParseQuestion(string answer, string variableName, out KeyValuePair<Guid, object> parsedValue);

        PreloadedDataDto[] CreatePreloadedDataDtosFromPanelData(PreloadedDataByFile[] allLevels);
        PreloadedDataDto[] CreatePreloadedDataDtoFromSampleData(PreloadedDataByFile sampleDataFile);
        string GetValidFileNameForTopLevelQuestionnaire();
    }
}
