﻿using System;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        private static ApplicationVersionSettings supportedQuestionnaireVersion;
        private static Func<bool> isDebugMode;
        private static Version buildVersion;

        public SupportedVersionProvider(ApplicationVersionSettings settings, Func<bool> isDebug, Version applicationBuildVersion)
        {
            supportedQuestionnaireVersion = settings;
            isDebugMode = isDebug;
            buildVersion = applicationBuildVersion;
        }

        public ApplicationVersionSettings GetSupportedQuestionnaireVersion()
        {
            return supportedQuestionnaireVersion;
        }

        public int? GetApplicationBuildNumber()
        {
            if (isDebugMode())
                return null;
            return buildVersion.Revision;
        }
    }
}
