using System;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    public interface IPreloadedDataVerifier
    {
        VerificationStatus VerifyAssignmentsSample(Guid questionnaireId, long version, PreloadedDataByFile data);
        VerificationStatus VerifyPanel(Guid questionnaireId, long version, PreloadedDataByFile[] data);
    }
}
