using System;
using Main.Core.Events.Questionnaire;
using NUnit.Framework;
using Ncqrs.Spec;

namespace Main.Core.Tests.Domain.QuestionnaireTests
{
    [TestFixture]
    public class DeleteImageTests : QuestionnaireARTestContext
    {

        [Test]
        public void DeleteImage_When_specified_keys_of_existing_question_and_image_Then_raised_ImageDeleted_event_with_specified_question_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var imageKey = Guid.NewGuid();
                var questionKey = Guid.NewGuid();
                var questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneQuestionAndOneImage(questionKey, imageKey);

                // act
                questionnaire.DeleteImage(questionKey, imageKey);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<ImageDeleted>(eventContext).QuestionKey, Is.EqualTo(questionKey));
            }
        }

        [Test]
        public void DeleteImage_When_specified_keys_of_existing_question_and_image_Then_raised_ImageDeleted_event_with_specified_image_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var imageKey = Guid.NewGuid();
                var questionKey = Guid.NewGuid();
                var questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneQuestionAndOneImage(questionKey, imageKey);

                // act
                questionnaire.DeleteImage(questionKey, imageKey);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<ImageDeleted>(eventContext).ImageKey, Is.EqualTo(imageKey));
            }
        }

    }
}