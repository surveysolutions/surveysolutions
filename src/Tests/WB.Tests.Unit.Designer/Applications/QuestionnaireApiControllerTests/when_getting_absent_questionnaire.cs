using System;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_getting_edit_question_info_and_question_is_absent : QuestionnaireApiControllerTestContext
    {
        [Test]
        public void should_throw_HttpResponseException_exception()
        {
            var controller = CreateQuestionnaireController();
            var actionResult = controller.EditQuestion(Create.QuestionnaireRevision(questionnaireId), questionId);
            Assert.That(actionResult, Is.InstanceOf(typeof(NotFoundResult)));
        }

        private static string questionnaireId = "22222222222222222222222222222222";
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}
