using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    [TestFixture]
    public class DeleteGroupTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void DeleteGroup_When_group_public_key_specified_Then_raised_GroupDeleted_event_with_same_group_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid groupPublicKey = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: responsibleId);

                // act
                Guid parentPublicKey = Guid.NewGuid();
                questionnaire.DeleteGroup(groupPublicKey, responsibleId: responsibleId);

                // assert
                Assert.That(GetSingleEvent<GroupDeleted>(eventContext).GroupPublicKey, Is.EqualTo(groupPublicKey));
            }
        }

        [Test]
        public void DeleteGroup_When_User_Doesnot_Have_Permissions_For_Edit_Questionnaire_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid groupPublicKey = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: Guid.NewGuid());

            // act
            TestDelegate act = () => questionnaire.DeleteGroup(groupPublicKey, responsibleId: Guid.NewGuid());
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

        [Test]
        public void DeleteGroup_When_Questions_Of_Group__Is_not_involved_in_the_validations_and_conditions_of_other_questions_outside_the_group_Then_DomainException_should_NOT_be_thrown()
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId);

            AddQuestion(questionnaire, Guid.NewGuid(), groupId, responsibleId, QuestionType.Text, "q1");

            // act
            TestDelegate act = () => questionnaire.DeleteGroup(groupId, responsibleId: responsibleId);

            // assert
            Assert.DoesNotThrow(act);
        }
    }
}