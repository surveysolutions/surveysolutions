using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.Tests.Integration.Versions
{
    internal class when_request_questionnaire_content_versions_of_designer_and_interviewer
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            designerEngineVersionService = new DesignerEngineVersionService(Mock.Of<IAttachmentService>());
            questionnaireContentVersionProvider = new QuestionnaireContentVersionProvider();
            BecauseOf();
        }

        public void BecauseOf() 
        {
            questionnaireContentVersion = questionnaireContentVersionProvider.GetSupportedQuestionnaireContentVersion().Major;
            designerLatestSupportedVersion = designerEngineVersionService.LatestSupportedVersion;
        }

        [NUnit.Framework.Test] public void should_return_same_versions_for_tester_version_and_designer_latest_supported_version () =>
            questionnaireContentVersion.Should().Be(designerLatestSupportedVersion);

        private static int questionnaireContentVersion;
        private static int designerLatestSupportedVersion;

        private static QuestionnaireContentVersionProvider questionnaireContentVersionProvider;
        private static DesignerEngineVersionService designerEngineVersionService;
    }
}
