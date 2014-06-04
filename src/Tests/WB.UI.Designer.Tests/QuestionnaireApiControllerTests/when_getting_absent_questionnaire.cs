using System;
using System.Net;
using System.Web;
using Machine.Specifications;
using WB.UI.Designer.Api;

namespace WB.UI.Designer.Tests.QuestionnaireApiControllerTests
{
    internal class when_getting_edit_question_info_and_question_is_absent : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
        };

        Because of = () =>
            exception = Catch.Exception(() => controller.EditQuestion(questionnaireId, questionId));

        It should_throw_exception = () =>
            exception.ShouldNotBeNull();

        It should_throw_HttpException_exception = () =>
            exception.ShouldBeOfExactType(typeof(HttpException));

        It should_throw_HttpException_exception1 = () =>
            (exception as HttpException).GetHttpCode().ShouldEqual((int)HttpStatusCode.NotFound);

        private static QuestionnaireController controller;
        private static Exception exception;
        private static string questionnaireId = "22222222222222222222222222222222";
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}