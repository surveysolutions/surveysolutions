using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Machine.Specifications;
using Machine.Specifications.Utility;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Api;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Shared.Web.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.ImportControllerTests
{
    internal class when_getting_Questionaire_but_user_dont_have_access_rights_to_use_the_questionnaire : ImportControllerTestContext
    {
        Establish context = () =>
        {
            request = Create.DownloadQuestionnaireRequest(questionnaireId);

            var membershipUserService = Mock.Of<IMembershipUserService>(
                _ => _.WebUser == Mock.Of<IMembershipWebUser>(
                    u => u.UserId == userId));

            var questionnaireViewFactory = Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(
                _ => _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) ==
                     Create.QuestionnaireView(questionnaireOwnerId));

            var sharedPersonsViewFactory = Mock.Of<IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons>>(
                    _ => _.Load(Moq.It.IsAny<QuestionnaireSharedPersonsInputModel>()) ==
                         Create.QuestionnaireSharedPersons(questionnaireId));

            importController = CreateImportController(membershipUserService: membershipUserService,
                questionnaireViewFactory: questionnaireViewFactory, sharedPersonsViewFactory: sharedPersonsViewFactory);
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

        private static ImportController importController;
        private static HttpResponseException exception;
        private static DownloadQuestionnaireRequest request;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static Guid questionnaireOwnerId = Guid.Parse("11111111111111111111111111111111");
        private static Guid userId = Guid.Parse("33333333333333333333333333333333");
    }
}
