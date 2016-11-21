using System;
using System.Net;
using System.Web.Http;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.Implementation;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Headquarters;
using WB.UI.Shared.Web.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class when_getting_Questionaire_but_user_dont_have_access_rights_to_use_the_questionnaire : ImportControllerTestContext
    {
        Establish context = () =>
        {
            request = Create.DownloadQuestionnaireRequest(questionnaireId);

            var membershipUserService = Mock.Of<IMembershipUserService>(
                _ => _.WebUser == Mock.Of<IMembershipWebUser>(
                    u => u.UserId == userId));

            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(
                _ => _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) ==
                     Create.QuestionnaireView(questionnaireOwnerId));

            importController = CreateImportController(membershipUserService: membershipUserService,
                questionnaireViewFactory: questionnaireViewFactory);
        };

        Because of = () =>
            exception = Catch.Only<HttpResponseException>(() =>
                importController.Questionnaire(request));

        It should_throw_HttpResponseException = () =>
            exception.ShouldNotBeNull();

        It should_throw_HttpResponseException_with_StatusCode_Forbidden = () =>
            exception.Response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);

        It should_throw_HttpResponseException_with_explanation_in_ReasonPhrase = () =>
            exception.Response.ReasonPhrase.ToLower().ToSeparateWords().ShouldContain("user", "not", "authorized", "check");

        private static ImportV2Controller importController;
        private static HttpResponseException exception;
        private static DownloadQuestionnaireRequest request;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static Guid questionnaireOwnerId = Guid.Parse("11111111111111111111111111111111");
        private static Guid userId = Guid.Parse("33333333333333333333333333333333");
    }
}
