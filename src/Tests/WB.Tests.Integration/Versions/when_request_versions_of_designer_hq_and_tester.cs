using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.Infrastructure.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.Versions
{
    internal class when_request_versions_of_designer_hq_and_tester
    {
        Establish context = () =>
        {
            designerEngineVersionService = new DesignerEngineVersionService();
            hqSupportedVersionProvider = new SupportedVersionProvider(new InMemoryPlainStorageAccessor<SupportedQuestionnaireVersion>());
        };

        Because of = () =>
        {
            designerLatestSupportedVersion = designerEngineVersionService.LatestSupportedVersion;
            hqVersion = hqSupportedVersionProvider.GetSupportedQuestionnaireVersion();
        };

        It should_return_same_versions_for_headquarters_version_and_designer_latest_supported_version = () =>
            hqVersion.ShouldEqual(designerLatestSupportedVersion);

        private static int designerLatestSupportedVersion;
        private static int hqVersion;

        private static DesignerEngineVersionService designerEngineVersionService;
        private static SupportedVersionProvider hqSupportedVersionProvider;
    }
}
