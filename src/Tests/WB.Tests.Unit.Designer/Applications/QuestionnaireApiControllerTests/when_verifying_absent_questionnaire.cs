using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_verifying_absent_questionnaire : QuestionnaireApiControllerTestContext
    {
        [Test]
        public void should_throw_HttpResponseException_exception()
        {
            var controller = CreateQuestionnaireController();
            var actionResult = controller.Verify(Create.QuestionnaireRevision(Id.g2));
            Assert.That(actionResult, Is.InstanceOf(typeof(NotFoundResult)));
        }
    }
}
