using System;
using System.Web;
using Machine.Specifications;
using WB.UI.Designer.Api;

namespace WB.UI.Designer.Tests.QuestionnaireApiControllerTests
{
    [Ignore("Should be fixed in KP-3405")]
    internal class when_getting_absent_chapter : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
        };

        Because of = () =>
            exception = Catch.Exception(() => controller.Chapter(questionnaireId, chapterId));

        It should_throw_exception = () =>
            exception.ShouldNotBeNull();

        It should_throw_HttpException_exception = () =>
            exception.ShouldBeOfExactType(typeof(HttpException));

        private static QuestionnaireController controller;
        private static Exception exception;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static string chapterId = "22222222222222222222222222222222";
    }
}