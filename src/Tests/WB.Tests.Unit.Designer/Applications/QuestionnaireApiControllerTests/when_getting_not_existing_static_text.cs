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
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
        };

        Because of = () =>
            exception = Catch.Exception(() => controller.EditStaticText(questionnaireId, entityId));

        It should_throw_exception = () =>
            exception.ShouldNotBeNull();

        It should_throw_HttpResponseException_exception = () =>
            exception.ShouldBeOfExactType(typeof(HttpResponseException));

        private static QuestionnaireController controller;
        private static Exception exception;
        private static string questionnaireId = "22222222222222222222222222222222";
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
    }
}