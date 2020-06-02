using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Tests.Abc;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Services
{
    public class WebTesterServiceTests
    {
        [Test]
        public void should_create_token_linked_to_questionnaire()
        {
            var subj = new WebTesterService(new MemoryCache(Options.Create(new MemoryCacheOptions())), Mock.Of<IOptions<WebTesterSettings>>(
                x => x.Value == new WebTesterSettings {ExpirationAmountMinutes = 1}));

            var token = subj.CreateTestQuestionnaire(Id.g1);

            var questionnaireId = subj.GetQuestionnaire(token);

            Assert.That(questionnaireId, Is.EqualTo(Id.g1));
        }
    }
}
