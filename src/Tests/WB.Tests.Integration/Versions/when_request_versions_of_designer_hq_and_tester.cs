﻿using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.UI.Designer.Api;

namespace WB.Tests.Integration.Versions
{
    internal class when_request_versions_of_designer_hq_and_tester
    {
        Establish context = () =>
        {
            designerEngineVersionService = new DesignerEngineVersionService();
            testerExpressionsEngineVersionService = new TesterExpressionsEngineVersionService();
            hqSupportedVersionProvider = new SupportedVersionProvider(() => false, new Version());
        };

        Because of = () =>
        {
            testerVersion = testerExpressionsEngineVersionService.GetExpressionsEngineSupportedVersion();
            designerLatestSupportedVersion = designerEngineVersionService.GetLatestSupportedVersion();
            designerApiVersion = QuestionnairesController.ApiVersion;
            hqVersion = hqSupportedVersionProvider.GetSupportedQuestionnaireVersion();
        };

        It should_return_same_versions_for_tester_version_and_designer_latest_supported_version = () =>
            testerVersion.ShouldEqual(designerLatestSupportedVersion);

        It should_return_same_versions_for_headquarters_version_and_designer_latest_supported_version = () =>
            hqVersion.ShouldEqual(designerLatestSupportedVersion);

        It should_return_same_versions_for_designer_api_version_and_designer_latest_supported_version = () =>
            designerApiVersion.ShouldEqual(designerLatestSupportedVersion);

        private static Version testerVersion;
        private static Version designerLatestSupportedVersion;
        private static Version designerApiVersion;
        private static Version hqVersion;

        private static TesterExpressionsEngineVersionService testerExpressionsEngineVersionService;
        private static DesignerEngineVersionService designerEngineVersionService;
        private static SupportedVersionProvider hqSupportedVersionProvider;
    }
}
