using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    public interface IPreloadedDataVerifier
    {
        IEnumerable<PanelImportVerificationError> VerifyAssignmentsSample(PreloadedDataByFile data, IPreloadedDataService dataService);
        IEnumerable<PanelImportVerificationError> VerifyPanelFiles(PreloadedDataByFile[] allLevels, IPreloadedDataService dataService);
        ImportDataInfo GetDetails(PreloadedDataByFile data);
        bool HasResponsibleNames(PreloadedDataByFile data);
    }
}
