using System;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        private static ApplicationVersionSettings supportedQuestionnaireVersion;
        private static Func<bool> isDebugMode;
        private static Version applicationBuildVersion;

        public SupportedVersionProvider(ApplicationVersionSettings settings, Func<bool> isDebug, Version applicationVersion)
        {
            supportedQuestionnaireVersion = settings;
            isDebugMode = isDebug;
            applicationBuildVersion = applicationVersion;
        }

        public ApplicationVersionSettings GetSupportedQuestionnaireVersion()
        {
            return supportedQuestionnaireVersion;
        }

        public int? GetApplicationBuildNumber()
        {
            if (isDebugMode())
                return null;

            return applicationBuildVersion.Revision;
        }
    }
}
