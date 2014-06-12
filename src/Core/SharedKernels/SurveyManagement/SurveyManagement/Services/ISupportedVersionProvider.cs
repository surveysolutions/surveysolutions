using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface ISupportedVersionProvider
    {
        QuestionnaireVersion GetSupportedQuestionnaireVersion();

        int? GetApplicationBuildNumber();
    }
}
