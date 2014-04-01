using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation
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
