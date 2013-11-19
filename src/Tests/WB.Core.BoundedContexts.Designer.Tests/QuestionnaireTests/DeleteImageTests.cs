using System;
using Main.Core.Domain;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Exceptions;

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
                var responsibleId = Guid.NewGuid();
                var questionnaire = CreateQuestionnaireWithOneQuestionAndOneImage(questionKey, imageKey, responsibleId: responsibleId);

                // act
                questionnaire.DeleteImage(questionKey, imageKey, responsibleId: responsibleId);

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
                var responsibleId = Guid.NewGuid();
                var questionnaire = CreateQuestionnaireWithOneQuestionAndOneImage(questionKey, imageKey, responsibleId: responsibleId);

                // act
                questionnaire.DeleteImage(questionKey, imageKey, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<ImageDeleted>(eventContext).ImageKey, Is.EqualTo(imageKey));
            }
        }

        [Test]
        public void DeleteImage_When_User_Doesnot_Have_Permissions_For_Edit_Questionnaire_Then_DomainException_should_be_thrown()
        {
            // arrange
            var imageKey = Guid.NewGuid();
            var questionKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireWithOneQuestionAndOneImage(questionKey, imageKey, responsibleId: Guid.NewGuid());

            // act
            TestDelegate act = () => questionnaire.DeleteImage(questionKey, imageKey, responsibleId: Guid.NewGuid());
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

    }
}