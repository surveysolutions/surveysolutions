using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Preloading
{
    public interface IPreloadedDataVerifier
    {
        VerificationStatus VerifySample(Guid questionnaireId, long version, PreloadedDataByFile data);
        VerificationStatus VerifyPanel(Guid questionnaireId, long version, PreloadedDataByFile[] data);
    }
}
