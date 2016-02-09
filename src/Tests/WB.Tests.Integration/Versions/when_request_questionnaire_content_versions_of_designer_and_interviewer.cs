using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;

namespace WB.Tests.Integration.Versions
{
    internal class when_request_questionnaire_content_versions_of_designer_and_interviewer
    {
        Establish context = () =>
        {
            designerEngineVersionService = new DesignerEngineVersionService();
            questionnaireContentVersionProvider = new QuestionnaireContentVersionProvider();
        };

        Because of = () =>
        {
            questionnaireContentVersion = questionnaireContentVersionProvider.GetSupportedQuestionnaireContentVersion();
            designerLatestSupportedVersion = designerEngineVersionService.GetLatestSupportedVersion();
        };

        It should_return_same_versions_for_tester_version_and_designer_latest_supported_version = () =>
            questionnaireContentVersion.ShouldEqual(designerLatestSupportedVersion.Major);

        private static long questionnaireContentVersion;
        private static Version designerLatestSupportedVersion;

        private static QuestionnaireContentVersionProvider questionnaireContentVersionProvider;
        private static DesignerEngineVersionService designerEngineVersionService;
    }
}