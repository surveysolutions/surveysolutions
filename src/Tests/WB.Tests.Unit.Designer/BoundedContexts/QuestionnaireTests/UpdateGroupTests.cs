using System;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [TestFixture]
    internal class UpdateGroupTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void NewUpdateGroup_When_groups_new_title_is_empty_or_whitespaces_Then_throws_DomainException(string emptyTitle)
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: groupPublicKey, responsibleId: responsibleId);

            // act
            TestDelegate act =
                () =>
                    questionnaire.UpdateGroup(groupPublicKey, responsibleId: responsibleId, title: emptyTitle, variableName: null, rosterSizeQuestionId: null,
                        description: null, condition: null, hideIfDisabled:false, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.GroupTitleRequired));
        }

        [Test]
        public void NewUpdateGroup_When_groups_new_title_is_not_empty_Then_raised_GroupUpdated_event_contains_the_same_group_title()
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: groupPublicKey, responsibleId: responsibleId);
            string notEmptyNewTitle = "Some new title";

            // act
            questionnaire.UpdateGroup(groupPublicKey, responsibleId: responsibleId, title: notEmptyNewTitle, variableName: null, rosterSizeQuestionId: null,
                description: null, condition: null, hideIfDisabled: false, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Find<IGroup>(groupPublicKey).Title, Is.EqualTo(notEmptyNewTitle));
        }

        [Test]
        public void NewUpdateGroup_When_group_does_not_exist_Then_throws_DomainException()
        {
            // arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            Guid notExistingGroupPublicKey = Guid.NewGuid();

            // act
            TestDelegate act = () =>
            {
                questionnaire.UpdateGroup(notExistingGroupPublicKey, responsibleId: responsibleId, title: null, variableName: null, rosterSizeQuestionId: null,
                    description: null, condition: null, hideIfDisabled: false, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);
            };

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.GroupNotFound));
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_Then_raised_GroupUpdated_event_contains_group_public_key()
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: responsibleId);

            // act
            questionnaire.UpdateGroup(groupPublicKey, responsibleId: responsibleId, title: "group text", variableName: null, rosterSizeQuestionId: null,
                description: null, condition: null, hideIfDisabled: false, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Find<IGroup>(groupPublicKey).PublicKey, Is.EqualTo(groupPublicKey));
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_and_group_text_specified_Then_raised_GroupUpdated_event_with_same_group_text()
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: responsibleId);
            var groupText = "new group text";

            // act
            questionnaire.UpdateGroup(groupPublicKey, responsibleId: responsibleId, title: groupText, variableName: null, rosterSizeQuestionId: null,
                description: null, condition: null, hideIfDisabled: false, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Find<IGroup>(groupPublicKey).Title, Is.EqualTo(groupText));
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_and_condition_expression_specified_Then_raised_GroupUpdated_event_with_same_condition_expression()
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: responsibleId);
            var conditionExpression = "2 < 7";

            // act
            questionnaire.UpdateGroup(groupPublicKey, responsibleId: responsibleId, title: "text of a group", variableName: null, rosterSizeQuestionId: null,
                description: null, condition: conditionExpression, hideIfDisabled: false, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Find<IGroup>(groupPublicKey).ConditionExpression, Is.EqualTo(conditionExpression));
        }

        [Test]
        public void NewUpdateGroup_When_group_exists_and_description_specified_Then_raised_GroupUpdated_event_with_same_description()
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: responsibleId);
            var description = "hardest questionnaire in the world";

            // act
            questionnaire.UpdateGroup(groupPublicKey, responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null,
                description: description, condition: null, hideIfDisabled: false, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Find<IGroup>(groupPublicKey).Description, Is.EqualTo(description));
        }

        [Test]
        public void NewUpdateGroup_When_User_Doesnot_Have_Permissions_For_Edit_Questionnaire_Then_DomainException_should_be_thrown()
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupPublicKey, responsibleId: Guid.NewGuid());
            var description = "hardest questionnaire in the world";

            // act
            TestDelegate act =
                () =>
                    questionnaire.UpdateGroup(groupPublicKey, responsibleId: Guid.NewGuid(), title: "Title", variableName: null, rosterSizeQuestionId: null,
                        description: description, condition: null, hideIfDisabled: false, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

        [Test]
        public void NewUpdateGroup_When_Group_Have_Condition_With_Reference_To_Existing_Question_Variable_Then_DomainException_should_NOT_be_thrown()
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
                    questionnaire.UpdateGroup(
                        groupId: groupId,
                        responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null, description: null, 
                        condition: expression, hideIfDisabled: false,
                        isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.DoesNotThrow(act);
        }

        [Test]
        public void NewUpdateGroup_When_Group_Have_Condition_With_Reference_To_Existing_Question_Id_Then_DomainException_should_NOT_be_thrown()
        {
            // arrange
            Guid questionId = Guid.NewGuid();
            Guid groupId = Guid.NewGuid();
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
                    questionnaire.UpdateGroup(
                        groupId: groupId,
                        responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null, description: null, 
                        condition: expression, hideIfDisabled: false,
                        isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.DoesNotThrow(act);
        }

        [Test]
        [Ignore("reference validation is turned off")]
        public void NewUpdateGroup_When_Group_Have_Condition_With_Reference_To_Not_Existing_Question_Then_DomainException_should_be_thrown()
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
                    questionnaire.UpdateGroup(
                        groupId: groupId,
                        responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null, description: null, 
                        condition: expression, hideIfDisabled: false,
                        isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.ExpressionContainsNotExistingQuestionOrRosterReference));
        }

        [Test]
        [Ignore("reference validation is turned off")]
        public void NewUpdateGroup_When_Group_Have_Condition_With_2_References_And_Second_Of_Them_To_Not_Existing_Question_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            string aliasForQuestion = "q1";
            string aliasForNotExistingQuestion = "q2";
            string expression = string.Format("[{0}] > 0 AND [{1}] > 1", aliasForQuestion, aliasForNotExistingQuestion);

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(expression) == new[] { aliasForQuestion, aliasForNotExistingQuestion });

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId, expressionProcessor: expressionProcessor);

            AddQuestion(questionnaire, Guid.NewGuid(), groupId, responsibleId, QuestionType.Text, aliasForQuestion);

            // act
            TestDelegate act =
                () =>
                    questionnaire.UpdateGroup(
                        groupId: groupId,
                        responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null, description: null, 
                        condition: expression, hideIfDisabled: false,
                        isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.ExpressionContainsNotExistingQuestionOrRosterReference));
        }
    }
}