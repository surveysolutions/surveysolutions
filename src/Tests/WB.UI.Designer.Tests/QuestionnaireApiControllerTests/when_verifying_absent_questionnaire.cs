using System;
using System.Web;
using Machine.Specifications;
using WB.UI.Designer.Api;

namespace WB.UI.Designer.Tests.QuestionnaireApiControllerTests
{
    [Ignore("Should be fixed in KP-3405")]
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

        It should_throw_HttpException_exception = () =>
            exception.ShouldBeOfExactType(typeof(HttpException));

        private static QuestionnaireController controller;
        private static Exception exception;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
    }
}