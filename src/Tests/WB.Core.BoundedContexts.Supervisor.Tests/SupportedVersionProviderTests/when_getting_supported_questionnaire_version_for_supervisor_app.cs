using Machine.Specifications;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.BoundedContexts.Supervisor.Tests.SupportedVersionProviderTests
{
    internal class when_getting_supported_questionnaire_version_for_supervisor_app : SupportedVersionProviderTestContext
    {
        Establish context = () =>
        {
            versionProvider = CreateSupportedVersionProvider();
        };

        Because of = () =>
            supportedVersion = versionProvider.GetSupportedQuestionnaireVersion();

        It should_set_Major_property_to_1 = () =>
            supportedVersion.Major.ShouldEqual(1);

        It should_set_Minor_property_to_6 = () =>
            supportedVersion.Minor.ShouldEqual(6);

        It should_set_Patch_property_to_0 = () =>
            supportedVersion.Patch.ShouldEqual(2);

        private static QuestionnaireVersion supportedVersion;
        private static SupportedVersionProvider versionProvider;

    }
}