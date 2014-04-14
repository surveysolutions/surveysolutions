using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Preloading
{
    internal interface IRosterDataService
    {
        HeaderStructureForLevel FindLevelInPreloadedData(PreloadedDataByFile levelData, QuestionnaireExportStructure exportStructure);
        Dictionary<Guid, string> CreateCleanedFileNamesForLevels(IDictionary<Guid, string> levels);
    }
}
