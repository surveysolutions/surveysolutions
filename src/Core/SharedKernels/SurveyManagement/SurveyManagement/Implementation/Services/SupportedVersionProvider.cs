using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        private static QuestionnaireVersion supportedQuestionnaireVersion;

        public SupportedVersionProvider(ApplicationVersionSettings settings)
        {
            supportedQuestionnaireVersion = new QuestionnaireVersion(
                settings.SupportedQuestionnaireVersionMajor,
                settings.SupportedQuestionnaireVersionMinor,
                settings.SupportedQuestionnaireVersionPatch);
        }

        public QuestionnaireVersion GetSupportedQuestionnaireVersion()
        {
            return supportedQuestionnaireVersion;
        }
    }
}
