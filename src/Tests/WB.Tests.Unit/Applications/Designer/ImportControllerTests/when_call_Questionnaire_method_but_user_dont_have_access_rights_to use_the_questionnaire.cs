using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Machine.Specifications;
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
    internal class when_call_Questionnaire_method_but_user_dont_have_access_rights_to_use_the_questionnaire : ImportControllerTestContext
    {
        Establish context = () =>
        {
            request = new DownloadQuestionnaireRequest() { QuestionnaireId = questionnaireId };

            var membershipUserService =
                Mock.Of<IMembershipUserService>(
                    _ => _.WebUser == Mock.Of<IMembershipWebUser>(u => u.UserId == userId));

            var questionnaireViewFactory =
                Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(
                    _ =>
                        _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) ==
                        new QuestionnaireView(new QuestionnaireDocument() {CreatedBy = questionnaireOwnerId}));

            var sharedPersonsViewFactory =
                Mock.Of<IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons>>(_ => _.Load(Moq.It.IsAny<QuestionnaireSharedPersonsInputModel>()) == new QuestionnaireSharedPersons(questionnaireId));

            importController = CreateImportController(membershipUserService: membershipUserService, questionnaireViewFactory: questionnaireViewFactory, sharedPersonsViewFactory: sharedPersonsViewFactory);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                importController.Questionnaire(request));

        It should_throw_HttpResponseException = () =>
            exception.ShouldBeOfExactType<HttpResponseException>();

        It should_throw_HttpResponseException_with_StatusCode_Forbidden = () =>
            ((HttpResponseException)exception).Response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);

        It should_throw_HttpResponseException_with_explanation_in_ReasonPhrase = () =>
            ((HttpResponseException)exception).Response.ReasonPhrase.ShouldEqual(
                    "User is not authorized. Please check your login and password.");

        private static ImportController importController;
        private static Exception exception;
        private static DownloadQuestionnaireRequest request;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static Guid questionnaireOwnerId = Guid.Parse("11111111111111111111111111111111");
        private static Guid userId = Guid.Parse("33333333333333333333333333333333");
    }
}
