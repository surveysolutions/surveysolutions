using System;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        private static ApplicationVersionSettings supportedQuestionnaireVersion;
        private static Func<bool> isDebugMode;
        private static int syncProtocolVersionNumber;

        public SupportedVersionProvider(ApplicationVersionSettings settings, Func<bool> isDebug, int syncProtocolVersion)
        {
            supportedQuestionnaireVersion = settings;
            isDebugMode = isDebug;
            syncProtocolVersionNumber = syncProtocolVersion;
        }

        public ApplicationVersionSettings GetSupportedQuestionnaireVersion()
        {
            return supportedQuestionnaireVersion;
        }

        public int? GetSupportedSyncProtocolVersionNumber()
        {
            if (isDebugMode())
                return null;

            return syncProtocolVersionNumber;
        }
    }
}
