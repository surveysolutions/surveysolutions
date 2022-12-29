using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [TestFixture]
    internal class SharedPersonsTests : QuestionnaireTestsContext
    { 
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
            var email = "string@empty.com";
            Guid personId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
                
            // act
            questionnaire.AddSharedPerson(personId, email, ShareType.Edit, responsibleId);
            questionnaire.RemoveSharedPerson(personId, responsibleId);

            // assert
            var isExistsPerson = questionnaire.SharedPersons.Any(p => p.UserId == personId);
            Assert.IsFalse(isExistsPerson);
        }

        [Test]
        public void RemoveSharedPerson_When_shared_person_is_read_only_Then_raised_ShredPersonFromQuestionnaireRemoved_event_with_specified_person_key()
        {
            // arrange
            var email = "string@empty.com";
            Guid personId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            // act
            questionnaire.AddSharedPerson(personId, email, ShareType.View, responsibleId);
            questionnaire.RemoveSharedPerson(personId, personId);

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
            TestDelegate act = () => questionnaire.RemoveSharedPerson(personId, responsibleId);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.UserDoesNotExistInShareList));
        }

        [Test]
        public void PassOwnership_should_be_available_only_to_questionnaire_owner()
        {
            // arrange
            Guid ownerId = Guid.NewGuid();
            Guid newOwnerId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: newOwnerId);

            // act
            TestDelegate act = () => questionnaire.TransferOwnership(ownerId, newOwnerId, string.Empty, string.Empty);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

        [Test]
        public void PassOwnership_should_be_available_only_to_questionnaire_owner_not_event_for_shared_for_edit()
        {
            // arrange
            Guid ownerId = Guid.NewGuid();
            Guid newOwnerId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: ownerId);

            questionnaire.AddSharedPerson(newOwnerId, "some@email", ShareType.Edit, ownerId);

            // act
            TestDelegate act = () => questionnaire.TransferOwnership(newOwnerId, ownerId, string.Empty, string.Empty);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

        [Test]
        public void PassOwnership_should_be_available_only_to_for_already_shared_user()
        {
            // arrange
            Guid ownerId = Guid.NewGuid();
            Guid newOwnerId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: ownerId);
            
            // act
            TestDelegate act = () => questionnaire.TransferOwnership(ownerId, newOwnerId, string.Empty, string.Empty);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.UserDoesNotExistInShareList));
        }

        [Test]
        public void PassOwnership_should_transfer_ownership_to_newOwner()
        {
            // arrange
            Guid ownerId = Guid.NewGuid();
            Guid newOwnerId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: ownerId);
            questionnaire.AddSharedPerson(newOwnerId, "newly@email", ShareType.Edit, ownerId);

            // act
            questionnaire.TransferOwnership(ownerId, newOwnerId, "oldy@email", "newly@email");

            // assert            
            Assert.That(questionnaire.QuestionnaireDocument.CreatedBy, Is.EqualTo(newOwnerId),
                "questionnaire property createdby is set to new owner id");

            Assert.That(questionnaire.SharedPersons.First(), Has.Property(nameof(SharedPerson.UserId)).EqualTo(ownerId),
                "only shared person left is the old owner of questionnaire");

            Assert.That(questionnaire.SharedPersons.First(), Has.Property(nameof(SharedPerson.ShareType)).EqualTo(ShareType.Edit),
                "old owner of questionnaire has edit permissions on questionnaire");
                        
            Assert.Null(questionnaire.SharedPersons.SingleOrDefault(p => p.UserId == newOwnerId),
                "questionnaire is no longer shared to new owner");
        }        
    }
}
