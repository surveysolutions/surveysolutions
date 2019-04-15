using System;
using System.Net;
using System.Web.Http;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Headquarters;


namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class when_getting_Questionaire_but_questionnaire_cant_be_verified : ImportControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            request = Create.DownloadQuestionnaireRequest(questionnaireId);
            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(
                _ => _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == Create.QuestionnaireView(userId));

            var expressionsEngineVersionService = Mock.Of<IDesignerEngineVersionService>(
                _ => _.IsClientVersionSupported(Moq.It.IsAny<int>()) == true && 
                _.GetListOfNewFeaturesForClient(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<int>()) == new string[0]);

            var questionnaireVerifier = Mock.Of<IQuestionnaireVerifier>(
                _ => _.CheckForErrors(Moq.It.IsAny<QuestionnaireView>()) ==
                     new[] {Create.VerificationError("code", "message") });

            importController = CreateImportController(
                questionnaireViewFactory: questionnaireViewFactory,
                engineVersionService: expressionsEngineVersionService,
                questionnaireVerifier: questionnaireVerifier);
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Assert.Throws<HttpResponseException>(() =>
                importController.Questionnaire(request));

        [NUnit.Framework.Test] public void should_throw_HttpResponseException () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_throw_HttpResponseException_with_StatusCode_PreconditionFailed () =>
            exception.Response.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);

        [NUnit.Framework.Test] public void should_throw_HttpResponseException_with_explanation_in_ReasonPhrase () =>
            exception.Response.ReasonPhrase.ToLower().ToSeparateWords().Should().Contain("questionnaire", "errors", "verify");

        private static ImportV2Controller importController;
        private static HttpResponseException exception;
        private static DownloadQuestionnaireRequest request;
        private static readonly Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid userId = Guid.Parse("33333333333333333333333333333333");
    }
}
