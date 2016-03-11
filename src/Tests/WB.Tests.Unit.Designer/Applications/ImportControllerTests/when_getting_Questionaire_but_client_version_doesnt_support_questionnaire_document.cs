using System;
using System.Net;
using System.Web.Http;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api;
using WB.UI.Shared.Web.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.ImportControllerTests
{
    internal class when_getting_Questionaire_but_client_version_doesnt_support_questionnaire_document : ImportControllerTestContext
    {
        Establish context = () =>
        {
            request = Create.DownloadQuestionnaireRequest(questionnaireId);

            var membershipUserService =
                Mock.Of<IMembershipUserService>(
                    _ => _.WebUser == Mock.Of<IMembershipWebUser>(u => u.UserId == userId));

            var questionnaireViewFactory = Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(
                _ => _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == Create.QuestionnaireView(userId));

            var expressionsEngineVersionService = Mock.Of<IDesignerEngineVersionService>(
                _ => _.IsClientVersionSupported(Moq.It.IsAny<Version>()) == true && _.IsQuestionnaireDocumentSupportedByClientVersion(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<Version>()) == false);

            importController = CreateImportController(membershipUserService: membershipUserService,
                questionnaireViewFactory: questionnaireViewFactory,
                engineVersionService: expressionsEngineVersionService);
        };

        Because of = () =>
            exception = Catch.Only<HttpResponseException>(() =>
                importController.Questionnaire(request));

        It should_throw_HttpResponseException = () =>
            exception.ShouldNotBeNull();

        It should_throw_HttpResponseException_with_StatusCode_UpgradeRequired = () =>
            exception.Response.StatusCode.ShouldEqual(HttpStatusCode.UpgradeRequired);

        It should_throw_HttpResponseException_with_explanation_in_ReasonPhrase = () =>
               exception.Response.ReasonPhrase.ToLower().ToSeparateWords().ShouldContain("questionnaire", "contains", "functionality", "not", "supported", "update");

        private static ImportV2Controller importController;
        private static HttpResponseException exception;
        private static DownloadQuestionnaireRequest request;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static Guid userId = Guid.Parse("33333333333333333333333333333333");
         
    }
}