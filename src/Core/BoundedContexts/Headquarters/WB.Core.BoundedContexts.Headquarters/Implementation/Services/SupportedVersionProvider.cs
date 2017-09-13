using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        private readonly IPlainKeyValueStorage<QuestionnaireVersion> appSettingsStorage;
        private static bool versionRemembered;

        public SupportedVersionProvider(IPlainKeyValueStorage<QuestionnaireVersion> appSettingsStorage)
        {
            this.appSettingsStorage = appSettingsStorage;
        }

        public int GetSupportedQuestionnaireVersion() => ApiVersion.MaxQuestionnaireVersion;
        public int? GetMinVerstionSupportedByInterviewer() => this.appSettingsStorage.GetById(QuestionnaireVersion.QuestionnaireVersionKey)?.MinQuestionnaireVersionSupportedByInterviewer;

        public void RememberMinSupportedVersion()
        {
            if (!versionRemembered)
            {
                var supportedQuestionnaireVersion = this.appSettingsStorage.GetById(QuestionnaireVersion.QuestionnaireVersionKey);
                if (supportedQuestionnaireVersion == null)
                {
                    this.appSettingsStorage.Store(new QuestionnaireVersion
                    {
                        MinQuestionnaireVersionSupportedByInterviewer = this.GetSupportedQuestionnaireVersion()
                    }, QuestionnaireVersion.QuestionnaireVersionKey);
                }

                versionRemembered = true;
            }
        }
    }
}
