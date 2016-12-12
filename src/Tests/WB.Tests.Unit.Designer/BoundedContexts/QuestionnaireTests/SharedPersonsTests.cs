using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [TestFixture]
    internal class SharedPersonsTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void AddSharedPerson_When_shared_personid_is_not_empty_Then_raised_SharedPersonToQuestionnaireAdded_event_with_specified_person_key()
        {
            // arrange
            Guid personId = Guid.NewGuid();
            string email = "unknown@u.com";
            Guid responsibleId = Guid.NewGuid();
            ShareType shareType = ShareType.View;
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
                
            // act
            questionnaire.AddSharedPerson(personId, email, shareType, responsibleId);

            // assert
            var person = questionnaire.SharedPersons.Single(p => p.UserId == personId);

            Assert.IsNotNull(person);
            Assert.That(person.UserId, Is.EqualTo(personId));
            Assert.That(person.Email, Is.EqualTo(email));
            Assert.That(person.ShareType, Is.EqualTo(shareType));
        }

        [Test]
        public void RemoveSharedPerson_When_shared_personid_is_not_empty_Then_raised_SharedPersonFromQuestionnaireRemoved_event_with_specified_person_key()
        {
            // arrange
            Guid personId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
                
            // act
            questionnaire.AddSharedPerson(personId, string.Empty, ShareType.Edit, responsibleId);
            questionnaire.RemoveSharedPerson(personId, string.Empty, responsibleId);

            // assert
            var isExistsPerson = questionnaire.SharedPersons.Any(p => p.UserId == personId);
            Assert.IsFalse(isExistsPerson);
        }

        [Test]
        public void AddSharedPerson_When_New_Shared_Person_Is_Questionnaire_Owner_Then_DomainException_should_be_thrown()
        {
            // arrange
            string email = "unknown@u.com";
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            
            // act
            TestDelegate act = () => questionnaire.AddSharedPerson(responsibleId, email, ShareType.Edit, responsibleId);
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
            questionnaire.AddSharedPerson(personId, email, ShareType.Edit, responsibleId);
            TestDelegate act = () => questionnaire.AddSharedPerson(personId, email, ShareType.Edit, responsibleId);
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