using System;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    [TestFixture]
    internal class CloneGroupTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void CloneGroupWithoutChildren_When_Group_Have_Condition_With_Variable_Name_Reference_To_Existing_Question_Then_DomainException_should_NOT_be_thrown()
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
                    questionnaire.CloneGroupWithoutChildren(
                        groupId: Guid.NewGuid(),
                        responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null, description: null, condition: expression,
                        parentGroupId: null, sourceGroupId: groupId, targetIndex: 1, isRoster: false,
                        rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.DoesNotThrow(act);
        }

        [Test]
        public void CloneGroupWithoutChildren_When_Group_Have_Condition_With_Question_Id_Reference_To_Existing_Question_Then_DomainException_should_NOT_be_thrown()
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            Guid validatedQuestion = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            string expression = string.Format("[{0}] > 0", validatedQuestion.ToString());

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(expression) == new[] { validatedQuestion.ToString() });

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId, expressionProcessor: expressionProcessor);

            AddQuestion(questionnaire, validatedQuestion, groupId, responsibleId, QuestionType.Text, "q2");

            // act
            TestDelegate act =
                () =>
                    questionnaire.CloneGroupWithoutChildren(
                        groupId: Guid.NewGuid(),
                        responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null, description: null, condition: expression,
                        parentGroupId: null, sourceGroupId: groupId, targetIndex: 1, isRoster: false,
                        rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            Assert.DoesNotThrow(act);
        }

        [Test]
        [Ignore("reference validation is turned off")]
        public void CloneGroupWithoutChildren_When_Group_Have_Condition_With_Reference_To_Not_Existing_Question_Then_DomainException_should_be_thrown()
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
                    questionnaire.CloneGroupWithoutChildren(
                        groupId: Guid.NewGuid(),
                        responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null, description: null, condition: expression,
                        parentGroupId: null, sourceGroupId: groupId, targetIndex: 1, isRoster: false,
                        rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.ExpressionContainsNotExistingQuestionOrRosterReference));
        }

        [Test]
        [Ignore("reference validation is turned off")]
        public void CloneGroupWithoutChildren_When_Group_Have_Condition_With_2_References_And_Second_Reference_To_Not_Existing_Question_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Guid questionId1 = Guid.NewGuid();
            Guid questionId2 = Guid.NewGuid();
            Guid idForNotExistingQuestion = Guid.NewGuid();
            string idForNotExistingQuestionAsString = idForNotExistingQuestion.ToString();

            string expression = string.Format("[{0}] > 0 AND [{1}] > 1", questionId1, questionId2);

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(expression) == new[] { questionId1.ToString(), idForNotExistingQuestionAsString });

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId, expressionProcessor: expressionProcessor);

            AddQuestion(questionnaire, questionId1, groupId, responsibleId, QuestionType.Text, "q1");
            AddQuestion(questionnaire, questionId2, groupId, responsibleId, QuestionType.Text, "q2");

            // act
            TestDelegate act =
                () =>
                    questionnaire.CloneGroupWithoutChildren(
                        groupId: Guid.NewGuid(),
                        responsibleId: responsibleId, title: "Title", variableName: null, rosterSizeQuestionId: null, description: null, condition: expression,
                        parentGroupId: null, sourceGroupId: groupId, targetIndex: 1, isRoster: false,
                        rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.ExpressionContainsNotExistingQuestionOrRosterReference));
            Assert.That(domainException.Message, Is.StringContaining(idForNotExistingQuestionAsString));
        }
    }

}
