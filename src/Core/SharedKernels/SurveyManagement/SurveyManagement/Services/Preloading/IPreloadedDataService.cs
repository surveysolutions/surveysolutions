using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Preloading
{
    internal interface IPreloadedDataService
    {
        HeaderStructureForLevel FindLevelInPreloadedData(string levelFileName);
        PreloadedDataByFile GetParentDataFile(string levelFileName, PreloadedDataByFile[] allLevels);
        decimal GetRecordIdValueAsDecimal(string[] dataFileRecord, int idColumnIndex);
        int GetIdColumnIndex(PreloadedDataByFile dataFile);
        int GetParentIdColumnIndex(PreloadedDataByFile dataFile);
        decimal[] GetAvalibleIdListForParent(PreloadedDataByFile parentDataFile, Guid levelId, string parentIdValue);
        Dictionary<string, int[]> GetColumnIndexesGoupedByQuestionVariableName(PreloadedDataByFile parentDataFile);
        KeyValuePair<Guid, object>? ParseQuestion(string answer, string variableName);
        PreloadedDataDto[] CreatePreloadedDataDto(PreloadedDataByFile[] allLevels);
    }
}
