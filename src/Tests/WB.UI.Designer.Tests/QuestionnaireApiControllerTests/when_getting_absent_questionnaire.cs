using System;
using System.Web;
using Machine.Specifications;
using WB.UI.Designer.Api;

namespace WB.UI.Designer.Tests.QuestionnaireApiControllerTests
{
    internal class when_getting_absent_questionnaire : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
        };

        Because of = () =>
            exception = Catch.Exception(() => controller.Get(questionnaireId));

        It should_throw_exception = () =>
            exception.ShouldNotBeNull();

        It should_throw_HttpException_exception = () =>
            exception.ShouldBeOfExactType(typeof(HttpException));

        private static QuestionnaireController controller;
        private static Exception exception;
        private static string questionnaireId = "22222222222222222222222222222222";
    }
}