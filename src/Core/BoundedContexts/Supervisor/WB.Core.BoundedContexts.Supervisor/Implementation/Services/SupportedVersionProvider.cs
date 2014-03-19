using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services
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
