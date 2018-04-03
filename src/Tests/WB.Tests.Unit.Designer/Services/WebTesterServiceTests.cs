using NUnit.Framework;
using WB.Tests.Abc;
using WB.UI.Designer.Implementation.Services;

namespace WB.Tests.Unit.Designer.Services
{
    public class WebTesterServiceTests
    {
        [Test]
        public void should_create_token_linked_to_questionnaire()
        {
            var subj = new WebTesterService( new WebTesterSettings {ExpirationAmountMinutes = 1});

            var token = subj.CreateTestQuestionnaire(Id.g1);

            var questionnaireId = subj.GetQuestionnaire(token);

            Assert.That(questionnaireId, Is.EqualTo(Id.g1));
        }
    }
}
