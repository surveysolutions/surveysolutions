using System;
using Main.Core.Domain;
using Main.Core.Events.Questionnaire;
using NUnit.Framework;
using Ncqrs.Spec;

namespace Main.Core.Tests.Domain.QuestionnaireTests
{
    [TestFixture]
    public class MoveQuestionnaireItemTests : QuestionnaireARTestContext
    {

        [Test]
        public void MoveQuestionnaireItem_When_public_key_specified_Then_raised_QuestionnaireItemMoved_event_with_same_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireAR();
                var publicKey = Guid.NewGuid();

                // act
                questionnaire.MoveQuestionnaireItem(publicKey, null, null);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<QuestionnaireItemMoved>(eventContext).PublicKey, Is.EqualTo(publicKey));
            }
        }

        [Test]
        public void MoveQuestionnaireItem_When_group_public_key_specified_Then_raised_QuestionnaireItemMoved_event_with_same_group_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireAR();
                var groupPublicKey = Guid.NewGuid();

                // act
                questionnaire.MoveQuestionnaireItem(Guid.NewGuid(), groupPublicKey, null);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<QuestionnaireItemMoved>(eventContext).GroupKey, Is.EqualTo(groupPublicKey));
            }
        }

        [Test]
        public void MoveQuestionnaireItem_When_public_key_of_item_to_put_after_specified_Then_raised_QuestionnaireItemMoved_event_with_same_public_key_of_item_to_put_after()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireAR();
                var afterItemPublicKey = Guid.NewGuid();

                // act
                questionnaire.MoveQuestionnaireItem(Guid.NewGuid(), null, afterItemPublicKey);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<QuestionnaireItemMoved>(eventContext).AfterItemKey, Is.EqualTo(afterItemPublicKey));
            }
        }
    }
}