using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        private static readonly QuestionnaireVersion supportedQuestionnaireVersion = new QuestionnaireVersion(1, 6, 0);

        public QuestionnaireVersion GetSupportedQuestionnaireVersion()
        {
            return supportedQuestionnaireVersion;
        }
    }
}
