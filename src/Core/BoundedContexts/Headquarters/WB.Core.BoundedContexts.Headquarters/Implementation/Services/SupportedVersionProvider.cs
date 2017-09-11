using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        private readonly IPlainKeyValueStorage<QuestionnaireVersion> questionnaireVersionStorage;
        private static bool versionRemembered;

        public SupportedVersionProvider(IPlainKeyValueStorage<QuestionnaireVersion> questionnaireVersionStorage)
        {
            this.questionnaireVersionStorage = questionnaireVersionStorage;
        }

        public int GetSupportedQuestionnaireVersion() => ApiVersion.MaxQuestionnaireVersion;
        public int? GetMinVerstionSupportedByInterviewer() => this.questionnaireVersionStorage.GetById(QuestionnaireVersion.QuestionnaireVersionKey)?.MinQuestionnaireVersionSupportedByInterviewer;

        public void RememberMinSupportedVersion()
        {
            if (!versionRemembered)
            {
                var supportedQuestionnaireVersion = this.questionnaireVersionStorage.GetById(QuestionnaireVersion.QuestionnaireVersionKey);
                if (supportedQuestionnaireVersion == null)
                {
                    this.questionnaireVersionStorage.Store(new QuestionnaireVersion
                    {
                        MinQuestionnaireVersionSupportedByInterviewer = this.GetSupportedQuestionnaireVersion()
                    }, QuestionnaireVersion.QuestionnaireVersionKey);
                }

                versionRemembered = true;
            }
        }
    }
}
