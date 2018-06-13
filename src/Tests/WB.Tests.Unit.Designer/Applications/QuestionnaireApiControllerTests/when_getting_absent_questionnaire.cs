using System;
using System.Web.Http;
using NUnit.Framework;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_getting_edit_question_info_and_question_is_absent : QuestionnaireApiControllerTestContext
    {
        [Test]
        public void should_throw_HttpResponseException_exception()
        {
            var controller = CreateQuestionnaireController();
            Assert.Throws<HttpResponseException>(() => controller.EditQuestion(questionnaireId, questionId));
        }

        private static string questionnaireId = "22222222222222222222222222222222";
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}
