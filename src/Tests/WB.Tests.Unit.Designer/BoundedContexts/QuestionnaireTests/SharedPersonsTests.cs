﻿using System;
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
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
                
            // act
            questionnaire.AddSharedPerson(personId, email, ShareType.Edit, responsibleId);

            // assert
            var evt = questionnaire.QuestionnaireDocument.SharedPersons.Find(p => p == personId);

            Assert.IsNotNull(evt);
//            Assert.That(evt.PersonId, Is.EqualTo(personId));
//            Assert.That(evt.Email, Is.EqualTo(email));
//            Assert.That(evt.ResponsibleId, Is.EqualTo(responsibleId));
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
            var person = questionnaire.QuestionnaireDocument.SharedPersons.FirstOrDefault(p => p == personId);
            Assert.That(person, Is.EqualTo(default(Guid)));
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