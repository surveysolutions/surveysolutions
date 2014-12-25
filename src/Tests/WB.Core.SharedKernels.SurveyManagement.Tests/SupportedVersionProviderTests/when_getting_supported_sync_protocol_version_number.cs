using System;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.SupportedVersionProviderTests
{
    internal class when_getting_supported_sync_protocol_version_number : SupportedVersionProviderTestContext
    {
        Establish context = () =>
        {
            versionProvider = CreateSupportedVersionProvider(new ApplicationVersionSettings
            {
                SupportedQuestionnaireVersionMajor = 1,
                SupportedQuestionnaireVersionMinor = 6,
                SupportedQuestionnaireVersionPatch = 2
            },applicationVersion:version );
        };

        Because of = () =>
            protocolVersionResult = versionProvider.GetApplicationBuildNumber();

        It should_return_version_which_was_provided_to_constructor = () =>
            protocolVersionResult.ShouldEqual(4);

        private static SupportedVersionProvider versionProvider;

        private static Version version = new Version(1, 2, 3, 4);

        private static int? protocolVersionResult;
        

    }
}