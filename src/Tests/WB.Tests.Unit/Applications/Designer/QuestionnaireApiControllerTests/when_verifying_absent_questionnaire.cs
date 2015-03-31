using System;
using System.Web.Http;
using Machine.Specifications;

using WB.UI.Designer.Api;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireApiControllerTests
{
    internal class when_verifying_absent_questionnaire : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
        };

        Because of = () =>
            exception = Catch.Exception(() => controller.Verify(questionnaireId));

        It should_throw_exception = () =>
            exception.ShouldNotBeNull();

        It should_throw_HttpResponseException_exception = () =>
            exception.ShouldBeOfExactType(typeof(HttpResponseException));

        private static QuestionnaireController controller;
        private static Exception exception;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
    }
}