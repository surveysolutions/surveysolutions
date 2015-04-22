using System;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface ISupportedVersionProvider
    {
        Version GetSupportedQuestionnaireVersion();

        int? GetApplicationBuildNumber();
    }
}
