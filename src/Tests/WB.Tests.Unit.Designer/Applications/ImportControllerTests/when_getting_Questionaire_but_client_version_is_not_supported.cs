using System;
using System.Net;
using System.Web.Http;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Headquarters;


namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class when_getting_Questionaire_but_client_version_is_not_supported : ImportControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            request = Create.DownloadQuestionnaireRequest(questionnaireId);

            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(
                _ => _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == Create.QuestionnaireView(userId));

            var expressionsEngineVersionService = Mock.Of<IDesignerEngineVersionService>(
                _ => _.IsClientVersionSupported(Moq.It.IsAny<int>()) == false);

            importController = CreateImportController(
                questionnaireViewFactory: questionnaireViewFactory,
                engineVersionService: expressionsEngineVersionService);
            importController.SetupLoggedInUser(userId);
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Assert.Throws<HttpResponseException>(() =>
                importController.Questionnaire(request));

        [NUnit.Framework.Test] public void should_throw_HttpResponseException () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_throw_HttpResponseException_with_StatusCode_UpgradeRequired () =>
            exception.Response.StatusCode.Should().Be(HttpStatusCode.UpgradeRequired);

        [NUnit.Framework.Test] public void should_throw_HttpResponseException_with_explanation_in_ReasonPhrase () =>
            exception.Response.ReasonPhrase.Should().Contain("You have an obsolete version of the Headquarters application. Please contact support@mysurvey.solutions to request an update.");

        private static ImportV2Controller importController;
        private static HttpResponseException exception;
        private static DownloadQuestionnaireRequest request;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static Guid userId = Guid.Parse("33333333333333333333333333333333");
    }
}
