using System;
using Moq;
using Ncqrs;
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
            var subj = new WebTesterService(Mock.Of<IClock>(), new WebTesterSettings {ExpirationAmountMinutes = 1});

            var token = subj.CreateTestQuestionnaire(Id.g1);

            var questionnaireId = subj.GetQuestionnaire(token);

            Assert.That(questionnaireId, Is.EqualTo(Id.g1));
        }

        [Test]
        public void should_expire_older_token_linked_to_questionnaire()
        {
            var subj = new WebTesterService(Mock.Of<IClock>(), new WebTesterSettings {ExpirationAmountMinutes = 1});

            var shouldBeExpiredToken = subj.CreateTestQuestionnaire(Id.g1);
            var token = subj.CreateTestQuestionnaire(Id.g1);

            Assert.Null(subj.GetQuestionnaire(shouldBeExpiredToken));

            var questionnaireId = subj.GetQuestionnaire(token);

            Assert.That(questionnaireId, Is.EqualTo(Id.g1));
        }

        [Test]
        public void should_not_return_expired_tokens_according_to_expiration_settings()
        {
            var subj = new WebTesterService(clock.Object, new WebTesterSettings {ExpirationAmountMinutes = 5});

            SetupClock(DateTime.UtcNow.AddMinutes(-10));
            var shouldExpireToken = subj.CreateTestQuestionnaire(Id.g1);
            SetupClock(DateTime.UtcNow);

            var questionnaireId = subj.GetQuestionnaire(shouldExpireToken);

            Assert.That(questionnaireId, Is.Null, "Token should be expired");

            SetupClock(DateTime.UtcNow.AddMinutes(-3));
            var notExpiredToken = subj.CreateTestQuestionnaire(Id.g1);
            SetupClock(DateTime.UtcNow);
            var questionnaire = subj.GetQuestionnaire(notExpiredToken);
            Assert.That(questionnaire, Is.EqualTo(Id.g1), "Token should not be expired");
        }

        [Test]
        public void should_cleanup_expired_tokens()
        {
            var subj = new WebTesterService(clock.Object, new WebTesterSettings { ExpirationAmountMinutes = 5 });

            SetupClock(DateTime.UtcNow.AddMinutes(-10));
            var token = subj.CreateTestQuestionnaire(Id.g1);
            SetupClock(DateTime.UtcNow);

            subj.Cleanup();
            SetupClock(DateTime.UtcNow.AddMinutes(-10));
            var questionnaire = subj.GetQuestionnaire(token);
            Assert.That(questionnaire, Is.Null, "Token should be expired");
        }

        private readonly Mock<IClock> clock = new Mock<IClock>();

        void SetupClock(DateTime value) => clock.Setup(c => c.UtcNow()).Returns(value);
    }
}
