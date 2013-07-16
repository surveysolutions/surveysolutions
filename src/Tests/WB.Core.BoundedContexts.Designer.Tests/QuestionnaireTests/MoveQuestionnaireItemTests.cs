using System;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    [TestFixture]
    public class MoveQuestionnaireItemTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void MoveQuestionnaireItem_When_public_key_specified_Then_raised_QuestionnaireItemMoved_event_with_same_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                var publicKey = Guid.NewGuid();

                // act
                questionnaire.MoveQuestionnaireItem(publicKey, null, null);

                // assert
                Assert.That(GetSingleEvent<QuestionnaireItemMoved>(eventContext).PublicKey, Is.EqualTo(publicKey));
            }
        }

        [Test]
        public void MoveQuestionnaireItem_When_group_public_key_specified_Then_raised_QuestionnaireItemMoved_event_with_same_group_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                var groupPublicKey = Guid.NewGuid();

                // act
                questionnaire.MoveQuestionnaireItem(Guid.NewGuid(), groupPublicKey, null);

                // assert
                Assert.That(GetSingleEvent<QuestionnaireItemMoved>(eventContext).GroupKey, Is.EqualTo(groupPublicKey));
            }
        }

        [Test]
        public void MoveQuestionnaireItem_When_public_key_of_item_to_put_after_specified_Then_raised_QuestionnaireItemMoved_event_with_same_public_key_of_item_to_put_after()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                var afterItemPublicKey = Guid.NewGuid();

                // act
                questionnaire.MoveQuestionnaireItem(Guid.NewGuid(), null, afterItemPublicKey);

                // assert
                Assert.That(GetSingleEvent<QuestionnaireItemMoved>(eventContext).AfterItemKey, Is.EqualTo(afterItemPublicKey));
            }
        }
    }
}