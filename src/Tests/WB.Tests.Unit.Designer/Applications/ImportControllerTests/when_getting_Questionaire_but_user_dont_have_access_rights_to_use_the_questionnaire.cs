using System;
using System.Net;
using System.Web.Http;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Headquarters;


namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class when_getting_Questionaire_but_user_dont_have_access_rights_to_use_the_questionnaire : ImportControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            request = Create.DownloadQuestionnaireRequest(questionnaireId);

            var membershipUserService = Mock.Of<IMembershipUserService>(
                _ => _.WebUser == Mock.Of<IMembershipWebUser>(
                    u => u.UserId == userId));

            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(
                _ => _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) ==
                     Create.QuestionnaireView(questionnaireOwnerId));

            importController = CreateImportController(membershipUserService: membershipUserService,
                questionnaireViewFactory: questionnaireViewFactory);
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Assert.Throws<HttpResponseException>(() =>
                importController.Questionnaire(request));

        [NUnit.Framework.Test] public void should_throw_HttpResponseException () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_throw_HttpResponseException_with_StatusCode_Forbidden () =>
            exception.Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        [NUnit.Framework.Test] public void should_throw_HttpResponseException_with_explanation_in_ReasonPhrase () =>
            exception.Response.ReasonPhrase.ToLower().ToSeparateWords().Should().Contain("user", "not", "authorized", "check");

        private static ImportV2Controller importController;
        private static HttpResponseException exception;
        private static DownloadQuestionnaireRequest request;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static Guid questionnaireOwnerId = Guid.Parse("11111111111111111111111111111111");
        private static Guid userId = Guid.Parse("33333333333333333333333333333333");
    }
}
