using System;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        private static Version expressionsEngineVersion = new Version(6, 0, 0);
        private static Func<bool> isDebugMode;
        private static Version applicationBuildVersion;

        public SupportedVersionProvider(Func<bool> isDebug, Version applicationVersion)
        {
            isDebugMode = isDebug;
            applicationBuildVersion = applicationVersion;
        }

        public Version GetSupportedQuestionnaireVersion()
        {
            return expressionsEngineVersion;
        }

        public int? GetApplicationBuildNumber()
        {
            if (isDebugMode())
                return null;

            return applicationBuildVersion.Revision;
        }
    }
}
