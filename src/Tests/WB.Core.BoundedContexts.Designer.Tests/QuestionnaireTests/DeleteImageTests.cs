using System;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    [TestFixture]
    public class DeleteImageTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void DeleteImage_When_specified_keys_of_existing_question_and_image_Then_raised_ImageDeleted_event_with_specified_question_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var imageKey = Guid.NewGuid();
                var questionKey = Guid.NewGuid();
                var questionnaire = CreateQuestionnaireWithOneQuestionAndOneImage(questionKey, imageKey);

                // act
                questionnaire.DeleteImage(questionKey, imageKey);

                // assert
                Assert.That(GetSingleEvent<ImageDeleted>(eventContext).QuestionKey, Is.EqualTo(questionKey));
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
                var questionnaire = CreateQuestionnaireWithOneQuestionAndOneImage(questionKey, imageKey);

                // act
                questionnaire.DeleteImage(questionKey, imageKey);

                // assert
                Assert.That(GetSingleEvent<ImageDeleted>(eventContext).ImageKey, Is.EqualTo(imageKey));
            }
        }

    }
}