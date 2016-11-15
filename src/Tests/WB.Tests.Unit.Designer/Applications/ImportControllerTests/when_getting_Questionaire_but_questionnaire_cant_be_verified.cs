using System;
using System.Net;
using System.Web.Http;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Headquarters;
using WB.UI.Shared.Web.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class when_getting_Questionaire_but_questionnaire_cant_be_verified : ImportControllerTestContext
    {
        Establish context = () =>
        {
            request = Create.DownloadQuestionnaireRequest(questionnaireId);

            var membershipUserService = Mock.Of<IMembershipUserService>(
                _ => _.WebUser == Mock.Of<IMembershipWebUser>(u => u.UserId == userId));

            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(
                _ => _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == Create.QuestionnaireView(userId));

            var expressionsEngineVersionService = Mock.Of<IDesignerEngineVersionService>(
                _ => _.IsClientVersionSupported(Moq.It.IsAny<int>()) == true && 
                _.GetListOfNewFeaturesForClient(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<int>()) == new string[0]);

            var questionnaireVerifier = Mock.Of<IQuestionnaireVerifier>(
                _ => _.CheckForErrors(Moq.It.IsAny<QuestionnaireView>()) ==
                     new[] {Create.VerificationError("code", "message") });

            importController = CreateImportController(membershipUserService: membershipUserService,
                questionnaireViewFactory: questionnaireViewFactory,
                engineVersionService: expressionsEngineVersionService,
                questionnaireVerifier: questionnaireVerifier);
        };

        Because of = () =>
            exception = Catch.Only<HttpResponseException>(() =>
                importController.Questionnaire(request));

        It should_throw_HttpResponseException = () =>
            exception.ShouldNotBeNull();

        It should_throw_HttpResponseException_with_StatusCode_PreconditionFailed = () =>
            exception.Response.StatusCode.ShouldEqual(HttpStatusCode.PreconditionFailed);

        It should_throw_HttpResponseException_with_explanation_in_ReasonPhrase = () =>
            exception.Response.ReasonPhrase.ToLower().ToSeparateWords().ShouldContain("questionnaire", "errors", "verify");

        private static ImportV2Controller importController;
        private static HttpResponseException exception;
        private static DownloadQuestionnaireRequest request;
        private static readonly Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid userId = Guid.Parse("33333333333333333333333333333333");
    }
}
