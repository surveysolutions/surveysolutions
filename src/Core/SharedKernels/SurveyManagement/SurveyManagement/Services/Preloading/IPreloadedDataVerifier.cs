using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Preloading
{
    public interface IPreloadedDataVerifier
    {
        IEnumerable<PreloadedDataVerificationError> Verify(Guid questionnaireId, long version, PreloadedDataByFile[] data);
    }
}
