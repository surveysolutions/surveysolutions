using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_getting_absent_chapter : QuestionnaireApiControllerTestContext
    {
        [Test]
        public void should_throw_HttpResponseException_exception()
        {
            var controller = CreateQuestionnaireController();
            var actionResult = controller.Chapter(questionnaireId, chapterId);
            Assert.That(actionResult, Is.InstanceOf(typeof(NotFoundResult)));
        }

        private static string chapterId = "22222222222222222222222222222222";
    }
}
