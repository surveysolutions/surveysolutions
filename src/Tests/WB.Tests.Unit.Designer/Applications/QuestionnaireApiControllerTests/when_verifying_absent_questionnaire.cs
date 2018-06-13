using System.Web.Http;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_verifying_absent_questionnaire : QuestionnaireApiControllerTestContext
    {
        [Test] public void should_throw_HttpResponseException_exception () {
            var controller = CreateQuestionnaireController();
            Assert.Throws<HttpResponseException>(() => controller.Verify(Id.g2));
        }
    }
}
