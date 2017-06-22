using System;
using System.Web.Http;
using Machine.Specifications;
using WB.UI.Designer.Api;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_verifying_absent_questionnaire : QuestionnaireApiControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateQuestionnaireController();
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() => controller.Verify(questionnaireId));

        [NUnit.Framework.Test] public void should_throw_exception () =>
            exception.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_throw_HttpResponseException_exception () =>
            exception.ShouldBeOfExactType(typeof(HttpResponseException));

        private static QuestionnaireController controller;
        private static Exception exception;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
    }
}