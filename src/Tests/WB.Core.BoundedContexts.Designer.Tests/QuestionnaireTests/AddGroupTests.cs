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
    public class AddGroupTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void NewAddGroup_When_groups_title_is_empty_or_whitespaces_Then_throws_DomainException(string emptyTitle)
        {
            // arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId : responsibleId);

            // act
            TestDelegate act = () => questionnaire.AddGroup(Guid.NewGuid(), responsibleId: responsibleId, title: emptyTitle, rosterSizeQuestionId: null, description: null, condition: null, parentGroupId: null);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.GroupTitleRequired));
        }

        [Test]
        public void NewAddGroup_When_groups_title_is_not_empty_Then_raised_NewAddGroup_event_contains_the_same_group_title()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaire(responsibleId : responsibleId);
                string notEmptyNewTitle = "Some new title";

                // act
                questionnaire.AddGroup(Guid.NewGuid(), responsibleId: responsibleId, title: notEmptyNewTitle, rosterSizeQuestionId: null, description: null, condition: null, parentGroupId: null);

                // assert
                Assert.That(GetSingleEvent<NewGroupAdded>(eventContext).GroupText, Is.EqualTo(notEmptyNewTitle));
            }
        }

        [Test]
        [Ignore("TLK KP-2834")]
        public void NewAddGroup_When_parent_group_is_roster_Then_throws_DomainException()
        {
            // arrange
            var parentAutoPropagateGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var rosterSizeQuestionId =       Guid.Parse("22222222-2222-2222-2222-222222222222");
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithRosterGroupAndQustion(rosterGroupId: parentAutoPropagateGroupId, rosterSizeQuestionId: rosterSizeQuestionId, responsibleId: responsibleId);

            // act
            TestDelegate act = () => questionnaire.AddGroup(Guid.NewGuid(), responsibleId: responsibleId, title: "Title", rosterSizeQuestionId: null, description: null, condition: null, parentGroupId: parentAutoPropagateGroupId);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.AutoPropagateGroupCantHaveChildGroups));
        }

        [Test]
        public void NewAddGroup_When_parent_group_is_non_propagated_Then_raised_NewAddGroup_event_contains_regular_group_id_as_parent()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var parentRegularGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneNonPropagatedGroup(groupId: parentRegularGroupId, responsibleId: responsibleId);

                // act
                questionnaire.AddGroup(Guid.NewGuid(), responsibleId: responsibleId, title: "Title", rosterSizeQuestionId: null, description: null, condition: null, parentGroupId: parentRegularGroupId);

                // assert
                Assert.That(GetLastEvent<NewGroupAdded>(eventContext).ParentGroupPublicKey, Is.EqualTo(parentRegularGroupId));
            }
        }

        [TestCase(Propagate.None)]
        [TestCase(Propagate.AutoPropagated)]
        public void NewAddGroup_When_User_Doesnot_Have_Permissions_For_Edit_Questionnaire_Then_DomainException_should_be_thrown(Propagate supportedPopagationKind)
        {
            // arrange
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: Guid.NewGuid());

            // act
            TestDelegate act = () => questionnaire.AddGroup(Guid.NewGuid(), responsibleId: Guid.NewGuid(), title: "Title", rosterSizeQuestionId: null, description: null, condition: null, parentGroupId: null);
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

        [Test]
        public void NewAddGroup_When_Group_Have_Condition_With_Reference_To_Existing_Question_Then_DomainException_should_NOT_be_thrown()
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId);
            string aliasForExistingQuestion = "q2";
            string expression = string.Format("[{0}] > 0", aliasForExistingQuestion);

            RegisterExpressionProcessorMock(expression, new[] { aliasForExistingQuestion });

            AddQuestion(questionnaire, Guid.NewGuid(), groupId, responsibleId, QuestionType.Text, aliasForExistingQuestion);

            // act
            TestDelegate act =
                () =>
                    questionnaire.AddGroup(
                        groupId: Guid.NewGuid(),
                        responsibleId: responsibleId, title: "Title", rosterSizeQuestionId: null, description: null, condition: expression, parentGroupId: groupId);

            // assert
            Assert.DoesNotThrow(act);
        }

        [Test]
        public void NewAddGroup_When_Group_Have_Condition_With_Reference_To_Not_Existing_Question_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId);
            string aliasForNotExistingQuestion = "q2";
            string expression = string.Format("[{0}] > 0", aliasForNotExistingQuestion);

            RegisterExpressionProcessorMock(expression, new[] { aliasForNotExistingQuestion });

            AddQuestion(questionnaire, Guid.NewGuid(), groupId, responsibleId, QuestionType.Text, "q1");

            // act
            TestDelegate act =
                () =>
                    questionnaire.AddGroup(
                        groupId: Guid.NewGuid(),
                        responsibleId: responsibleId, title: "Title", rosterSizeQuestionId: null, description: null, condition: expression, parentGroupId: groupId);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.ExpressionContainsNotExistingQuestionReference));
        }
    }
}