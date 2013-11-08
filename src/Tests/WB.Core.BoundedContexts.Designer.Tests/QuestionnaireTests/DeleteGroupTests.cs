using System;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
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
    public class DeleteGroupTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);
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
                questionnaire.NewDeleteGroup(groupPublicKey, responsibleId: responsibleId);

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
            TestDelegate act = () => questionnaire.NewDeleteGroup(groupPublicKey, responsibleId: Guid.NewGuid());
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
            TestDelegate act = () => questionnaire.NewDeleteGroup(groupId, responsibleId: responsibleId);

            // assert
            Assert.DoesNotThrow(act);
        }

        [Test]
        public void DeleteGroup_When_Some_Question_Of_Group_involved_in_the_condition_of_another_question_outside_the_group_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid firstGroupId = Guid.NewGuid();
            Guid secondGroup = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithTwoGroups(responsibleId: responsibleId,
                firstGroup: firstGroupId, secondGroup: secondGroup);
            string aliasForQuestionInGroup = "q1";
            string expression = string.Format("[{0}] > 0", aliasForQuestionInGroup);

            RegisterExpressionProcessorMock(expression, new[] { aliasForQuestionInGroup });

            AddQuestion(questionnaire, Guid.NewGuid(), firstGroupId, responsibleId, QuestionType.Text, aliasForQuestionInGroup);
            AddQuestion(questionnaire, Guid.NewGuid(), secondGroup, responsibleId, QuestionType.Text, "q2",
                condition: expression);

            // act
            TestDelegate act = () => questionnaire.NewDeleteGroup(firstGroupId, responsibleId: responsibleId);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionOrGroupDependOnAnotherQuestion));
        }

        [Test]
        public void DeleteGroup_When_Some_Question_Of_Group_involved_in_the_validation_of_another_question_outside_the_group_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid firstGroupId = Guid.NewGuid();
            Guid secondGroup = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithTwoGroups(responsibleId: responsibleId,
                firstGroup: firstGroupId, secondGroup: secondGroup);
            string aliasForQuestionInGroup = "q1";
            string expression = string.Format("[{0}] > 0", aliasForQuestionInGroup);

            RegisterExpressionProcessorMock(expression, new[] { aliasForQuestionInGroup });

            AddQuestion(questionnaire, Guid.NewGuid(), firstGroupId, responsibleId, QuestionType.Text, aliasForQuestionInGroup);
            AddQuestion(questionnaire, Guid.NewGuid(), secondGroup, responsibleId, QuestionType.Text, "q2",
                validation: expression);

            // act
            TestDelegate act = () => questionnaire.NewDeleteGroup(firstGroupId, responsibleId: responsibleId);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionOrGroupDependOnAnotherQuestion));
        }

        [Test]
        public void DeleteGroup_When_Some_Question_Of_Group_involved_in_the_condition_of_another_group_outside_the_group_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid firstGroupId = Guid.NewGuid();
            Guid secondGroupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: firstGroupId);
            string aliasForQuestionInGroup = "q1";
            string expression = string.Format("[{0}] > 0", aliasForQuestionInGroup);

            RegisterExpressionProcessorMock(expression, new[] { aliasForQuestionInGroup });

            AddQuestion(questionnaire, Guid.NewGuid(), firstGroupId, responsibleId, QuestionType.Text, aliasForQuestionInGroup);
            AddGroup(questionnaire: questionnaire, groupId: secondGroupId, parentGroupId: null, responsibleId: responsibleId,
                condition: expression);
            
            // act
            TestDelegate act = () => questionnaire.NewDeleteGroup(firstGroupId, responsibleId: responsibleId);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionOrGroupDependOnAnotherQuestion));
        }
    }
}