using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        private readonly IPlainStorageAccessor<SupportedQuestionnaireVersion> supportedVersionStorage;
        private const string id = "MinVersion";
        private static bool versionRemembered;

        public SupportedVersionProvider(IPlainStorageAccessor<SupportedQuestionnaireVersion> supportedVersionStorage)
        {
            this.supportedVersionStorage = supportedVersionStorage;
        }

        public int GetSupportedQuestionnaireVersion() => ApiVersion.MaxQuestionnaireVersion;
        public int GetMinVerstionSupportedByInterviewer() => this.supportedVersionStorage.GetById(id).MinQuestionnaireVersionSupportedByInterviewer;

        public void RememberMinSupportedVersion()
        {
            if (!versionRemembered)
            {
                var supportedQuestionnaireVersion = this.supportedVersionStorage.GetById(id);
                if (supportedQuestionnaireVersion == null)
                {
                    this.supportedVersionStorage.Store(new SupportedQuestionnaireVersion
                    {
                        Id = id,
                        MinQuestionnaireVersionSupportedByInterviewer = this.GetSupportedQuestionnaireVersion()
                    }, id);
                }

                versionRemembered = true;
            }
        }
    }
}
