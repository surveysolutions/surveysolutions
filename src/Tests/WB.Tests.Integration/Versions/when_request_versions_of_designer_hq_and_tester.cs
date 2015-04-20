using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Tests.Integration.Versions
{
    internal class when_request_versions_of_designer_hq_and_tester
    {
        Establish context = () =>
        {
            designerExpressionsEngineVersionService = new ExpressionsEngineVersionService();
            testerExpressionsEngineVersionService = new CapiExpressionsEngineVersionService();
            hqSupportedVersionProvider = new SupportedVersionProvider(() => false, new Version());
        };

        Because of = () =>
        {
            testerVersion = testerExpressionsEngineVersionService.GetExpressionsEngineSupportedVersion();
            designerVersion = designerExpressionsEngineVersionService.GetLatestSupportedVersion();
            hqVersion = hqSupportedVersionProvider.GetSupportedQuestionnaireVersion();
        };

        It should_designer_Major_version_be_equal_to_tester_Major_version = () =>
            designerVersion.Major.ShouldEqual(testerVersion.Major);

        It should_designer_Minor_version_be_equal_to_tester_Minor_version = () =>
            designerVersion.Minor.ShouldEqual(testerVersion.Minor);

        It should_designer_Build_version_be_equal_to_tester_Patch_version = () =>
            designerVersion.Build.ShouldEqual(testerVersion.Patch);

        It should_designer_Major_version_be_equal_to_hq_Major_version = () =>
           designerVersion.Major.ShouldEqual(hqVersion.Major);

        It should_designer_Minor_version_be_equal_to_hq_Minor_version = () =>
            designerVersion.Minor.ShouldEqual(hqVersion.Minor);

        It should_designer_Build_version_be_equal_to_hq_Build_version = () =>
            designerVersion.Build.ShouldEqual(hqVersion.Build);

        private static QuestionnnaireVersion testerVersion;
        private static Version designerVersion;
        private static Version hqVersion;

        private static ICapiExpressionsEngineVersionService testerExpressionsEngineVersionService;
        private static IExpressionsEngineVersionService designerExpressionsEngineVersionService;
        private static ISupportedVersionProvider hqSupportedVersionProvider;
    }
}
