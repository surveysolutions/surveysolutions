using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.SupportedVersionProviderTests
{
    internal class when_getting_supported_questionnaire_version_for_supervisor_app : SupportedVersionProviderTestContext
    {
        Establish context = () =>
        {
            versionProvider = CreateSupportedVersionProvider(new ApplicationVersionSettings
            {
                SupportedQuestionnaireVersionMajor = 1,
                SupportedQuestionnaireVersionMinor = 6,
                SupportedQuestionnaireVersionPatch = 2
            });
        };

        Because of = () =>
            supportedVersion = versionProvider.GetSupportedQuestionnaireVersion();

        It should_set_Major_property_to_1 = () =>
            supportedVersion.SupportedQuestionnaireVersionMajor.ShouldEqual(1);

        It should_set_Minor_property_to_6 = () =>
            supportedVersion.SupportedQuestionnaireVersionMinor.ShouldEqual(6);

        It should_set_Patch_property_to_0 = () =>
            supportedVersion.SupportedQuestionnaireVersionPatch.ShouldEqual(2);

        private static ApplicationVersionSettings supportedVersion;
        private static SupportedVersionProvider versionProvider;

    }
}