using System;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [TestFixture]
    internal class AddGroupTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void NewAddGroup_When_groups_title_is_empty_or_whitespaces_Then_throws_DomainException(string emptyTitle)
        {
            // arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            // act
            TestDelegate act =
                () =>
                    questionnaire.AddGroupAndMoveIfNeeded(Guid.NewGuid(), responsibleId: responsibleId, title: emptyTitle, variableName: null, rosterSizeQuestionId: null,
                        description: null, condition: null, hideIfDisabled:false, parentGroupId: null, isRoster: false,
                        rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.GroupTitleRequired));
        }

        [Test]
        public void NewAddGroup_When_groups_title_is_not_empty_Then_raised_NewAddGroup_event_contains_the_same_group_title()
        {
            // arrange
            Guid responsibleId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            string notEmptyNewTitle = "Some new title";

            // act
            questionnaire.AddGroupAndMoveIfNeeded(groupId, responsibleId: responsibleId, title: notEmptyNewTitle, variableName: null, rosterSizeQuestionId: null,
                description: null, condition: null, hideIfDisabled: false, parentGroupId: null, isRoster: false,
                rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).Title, Is.EqualTo(notEmptyNewTitle));
        }

        [Test]
        public void NewAddGroup_When_parent_group_is_not_roster_Then_raised_NewAddGroup_event_contains_regular_group_id_as_parent()
        {
            // arrange
            var parentRegularGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Guid responsibleId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneNotRosterGroup(groupId: parentRegularGroupId, responsibleId: responsibleId);

            // act
            questionnaire.AddGroupAndMoveIfNeeded(groupId, responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null,
                description: null, condition: null, hideIfDisabled: false, parentGroupId: parentRegularGroupId, isRoster: false,
                rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).GetParent().PublicKey, Is.EqualTo(parentRegularGroupId));
        }

        public void NewAddGroup_When_User_Doesnot_Have_Permissions_For_Edit_Questionnaire_Then_DomainException_should_be_thrown()
        {
            // arrange
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: Guid.NewGuid());

            // act
            TestDelegate act =
                () =>
                    questionnaire.AddGroupAndMoveIfNeeded(Guid.NewGuid(), responsibleId: Guid.NewGuid(), title: "Title", variableName: null, rosterSizeQuestionId: null,
                        description: null, condition: null, hideIfDisabled: false, parentGroupId: null, isRoster: false,
                        rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

        public void NewAddGroup_When_User_In_Readonly_mode_Questionnaire_Then_DomainException_should_be_thrown()
        {
            // arrange
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: Guid.NewGuid());
            var readOnlyUser = Guid.NewGuid();
            questionnaire.AddSharedPersonToQuestionnaire(new SharedPersonToQuestionnaireAdded()
            {
                PersonId = readOnlyUser,
                ShareType = ShareType.View
            });
            // act
            TestDelegate act =
                () =>
                    questionnaire.AddGroupAndMoveIfNeeded(Guid.NewGuid(), responsibleId: readOnlyUser, title: "Title", variableName: null, rosterSizeQuestionId: null,
                        description: null, condition: null, hideIfDisabled: false, parentGroupId: null, isRoster: false,
                        rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

        [Test]
        public void NewAddGroup_When_Group_Have_Condition_With_Reference_To_Existing_Question_Variable_Then_DomainException_should_NOT_be_thrown()
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            string aliasForExistingQuestion = "q2";
            string expression = string.Format("[{0}] > 0", aliasForExistingQuestion);

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(expression) == new[] { aliasForExistingQuestion });

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId, expressionProcessor: expressionProcessor);

            AddQuestion(questionnaire, Guid.NewGuid(), groupId, responsibleId, QuestionType.Text, aliasForExistingQuestion);

            // act
            TestDelegate act =
                () =>
                    questionnaire.AddGroupAndMoveIfNeeded(
                        groupId: Guid.NewGuid(),
                        responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null, description: null, 
                        condition: expression, hideIfDisabled: false,
                        parentGroupId: groupId, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.DoesNotThrow(act);
        }

        [Test]
        public void NewAddGroup_When_Group_Have_Condition_With_Reference_To_Existing_Question_Id_Then_DomainException_should_NOT_be_thrown()
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            Guid questionId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();

            string expression = string.Format("[{0}] > 0", questionId);

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(expression) == new[] { questionId.ToString() });

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId, expressionProcessor: expressionProcessor);

            AddQuestion(questionnaire, questionId, groupId, responsibleId, QuestionType.Text, "q1");

            // act
            TestDelegate act =
                () =>
                    questionnaire.AddGroupAndMoveIfNeeded(
                        groupId: Guid.NewGuid(),
                        responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null, description: null, 
                        condition: expression, hideIfDisabled: false,
                        parentGroupId: groupId, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.DoesNotThrow(act);
        }

        [Test]
        [Ignore("reference validation is turned off")]
        public void NewAddGroup_When_Group_Have_Condition_With_Reference_To_Not_Existing_Question_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            string aliasForNotExistingQuestion = "q2";
            string expression = string.Format("[{0}] > 0", aliasForNotExistingQuestion);

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(expression) == new[] { aliasForNotExistingQuestion });

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId, expressionProcessor: expressionProcessor);

            AddQuestion(questionnaire, Guid.NewGuid(), groupId, responsibleId, QuestionType.Text, "q1");

            // act
            TestDelegate act =
                () =>
                    questionnaire.AddGroupAndMoveIfNeeded(
                        groupId: Guid.NewGuid(),
                        responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null, description: null, 
                        condition: expression, hideIfDisabled: false,
                        parentGroupId: groupId, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.ExpressionContainsNotExistingQuestionOrRosterReference));
        }

        [Test]
        [Ignore("reference validation is turned off")]
        public void NewAddGroup_When_Group_Have_Condition_With_2_References_And_Second_Of_Them_To_Not_Existing_Question_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            string aliasForQuestion1 = "q1";
            string aliasForNotExistingQuestion = "q2";
            string expression = string.Format("[{0}] > 0 AND [{1}] > 1", aliasForQuestion1, aliasForNotExistingQuestion);

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(expression) == new[] { aliasForQuestion1, aliasForNotExistingQuestion });

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId, expressionProcessor: expressionProcessor);

            AddQuestion(questionnaire, Guid.NewGuid(), groupId, responsibleId, QuestionType.Text, aliasForQuestion1);

            // act
            TestDelegate act =
                () =>
                    questionnaire.AddGroupAndMoveIfNeeded(
                        groupId: Guid.NewGuid(),
                        responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null, description: null,
                        condition: expression, hideIfDisabled: false,
                        parentGroupId: groupId, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.ExpressionContainsNotExistingQuestionOrRosterReference));
            Assert.That(domainException.Message, Is.StringContaining(aliasForNotExistingQuestion));
        }
    }
}