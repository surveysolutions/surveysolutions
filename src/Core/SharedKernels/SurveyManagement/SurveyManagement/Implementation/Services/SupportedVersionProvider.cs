using System;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        private static readonly Version ExpressionsEngineVersion = new Version(9, 0, 0);
        private static Func<bool> isDebugMode;
        private static Version applicationBuildVersion;

        public SupportedVersionProvider(Func<bool> isDebug, Version applicationVersion)
        {
            isDebugMode = isDebug;
            applicationBuildVersion = applicationVersion;
        }

        public Version GetSupportedQuestionnaireVersion()
        {
            return ExpressionsEngineVersion;
        }

        public int? GetApplicationBuildNumber()
        {
            if (isDebugMode())
                return null;

            return applicationBuildVersion.Revision;
        }
    }
}
