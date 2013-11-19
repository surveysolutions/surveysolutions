using System;
using Main.Core.Domain;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    [TestFixture]
    public class SharedPersonsTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void AddSharedPerson_When_shared_personid_is_not_empty_Then_raised_SharedPersonToQuestionnaireAdded_event_with_specified_person_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid personId = Guid.NewGuid();
                string email = "unknown@u.com";
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
                
                // act
                questionnaire.AddSharedPerson(personId, email, responsibleId);

                // assert
                var evt = GetSingleEvent<SharedPersonToQuestionnaireAdded>(eventContext);

                Assert.IsNotNull(evt);
                Assert.That(evt.PersonId, Is.EqualTo(personId));
                Assert.That(evt.Email, Is.EqualTo(email));
                Assert.That(evt.ResponsibleId, Is.EqualTo(responsibleId));
            }
        }

        [Test]
        public void RemoveSharedPerson_When_shared_personid_is_not_empty_Then_raised_SharedPersonFromQuestionnaireRemoved_event_with_specified_person_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid personId = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
                
                // act
                questionnaire.AddSharedPerson(personId, string.Empty, responsibleId);
                questionnaire.RemoveSharedPerson(personId, string.Empty, responsibleId);

                // assert
                var evt = GetSingleEvent<SharedPersonFromQuestionnaireRemoved>(eventContext);
                Assert.IsNotNull(evt);
                Assert.That(evt.PersonId, Is.EqualTo(personId));
                Assert.That(evt.ResponsibleId, Is.EqualTo(responsibleId));
            }
        }

        [Test]
        public void AddSharedPerson_When_New_Shared_Person_Is_Questionnaire_Owner_Then_DomainException_should_be_thrown()
        {
            // arrange
            string email = "unknown@u.com";
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            
            // act
            TestDelegate act = () => questionnaire.AddSharedPerson(responsibleId, email, responsibleId);
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.OwnerCannotBeInShareList));
        }

        [Test]
        public void AddSharedPerson_When_New_Shared_Person_Already_Exist_In_Share_List_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid personId = Guid.NewGuid();
            string email = "unknown@u.com";
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            // act
            questionnaire.AddSharedPerson(personId, email, responsibleId);
            TestDelegate act = () => questionnaire.AddSharedPerson(personId, email, responsibleId);
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.UserExistInShareList));
        }

        [Test]
        public void RemoveSharedPerson_When_Share_List_Doesnot_Exist_Shared_User_For_Deleting_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid personId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            // act
            TestDelegate act = () => questionnaire.RemoveSharedPerson(personId, string.Empty, responsibleId);
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.UserDoesNotExistInShareList));
        }
    }
}