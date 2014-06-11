using System;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        private static QuestionnaireVersion supportedQuestionnaireVersion;
        private static bool isDebugMode;
        private static Version buildVersion;

        public SupportedVersionProvider(ApplicationVersionSettings settings, bool isDebug, Version applicationBuildVersion)
        {
            supportedQuestionnaireVersion = new QuestionnaireVersion(
                settings.SupportedQuestionnaireVersionMajor,
                settings.SupportedQuestionnaireVersionMinor,
                settings.SupportedQuestionnaireVersionPatch);

            isDebugMode = isDebug;
            buildVersion = applicationBuildVersion;
        }

        public QuestionnaireVersion GetSupportedQuestionnaireVersion()
        {
            return supportedQuestionnaireVersion;
        }

        public int? GetApplicationBuildNumber()
        {
            if (isDebugMode)
                return null;
            return buildVersion.Revision;
        }
    }
}
