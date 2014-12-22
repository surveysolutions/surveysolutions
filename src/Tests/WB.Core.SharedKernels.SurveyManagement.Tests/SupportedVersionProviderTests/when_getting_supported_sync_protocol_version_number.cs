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
            }, syncProtocolVersion: ProtocolVersion);
        };

        Because of = () =>
            protocolVersionResult = versionProvider.GetSupportedSyncProtocolVersionNumber();

        It should_protocolVersion_equals_provided = () =>
            protocolVersionResult.ShouldEqual(ProtocolVersion);

        private static SupportedVersionProvider versionProvider;

        private const int ProtocolVersion = 44;

        private static int? protocolVersionResult;
        

    }
}