using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface ISupportedVersionProvider
    {
        ApplicationVersionSettings GetSupportedQuestionnaireVersion();

        int? GetApplicationBuildNumber();
    }
}
