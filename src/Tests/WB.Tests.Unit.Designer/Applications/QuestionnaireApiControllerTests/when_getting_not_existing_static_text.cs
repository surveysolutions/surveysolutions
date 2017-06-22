using System;
using System.Net;
using System.Web;
using System.Web.Http;
using Machine.Specifications;
using WB.UI.Designer.Api;

namespace WB.UI.Designer.Tests.QuestionnaireApiControllerTests
{
    internal class when_getting_not_existing_static_text : QuestionnaireApiControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateQuestionnaireController();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() => controller.EditStaticText(questionnaireId, entityId));

        [NUnit.Framework.Test] public void should_throw_exception () =>
            exception.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_throw_HttpResponseException_exception () =>
            exception.ShouldBeOfExactType(typeof(HttpResponseException));

        private static QuestionnaireController controller;
        private static Exception exception;
        private static string questionnaireId = "22222222222222222222222222222222";
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
    }
}