using System;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    public interface IPreloadedDataVerifier
    {
        ImportDataVerificationState VerifyAssignmentsSample(Guid questionnaireId, long version, PreloadedDataByFile data);
        void VerifyPanelFiles(Guid questionnaireId, long version, PreloadedDataByFile[] data, AssignmentImportStatus status);
    }
}
