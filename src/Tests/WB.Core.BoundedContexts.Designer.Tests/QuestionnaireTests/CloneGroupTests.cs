﻿using System;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.GenericSubdomains.Logging;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    [TestFixture]
    public class CloneGroupTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            NcqrsEnvironment.SetGetter<ILogger>(Mock.Of<ILogger>);
            NcqrsEnvironment.SetGetter<IUniqueIdentifierGenerator>(Mock.Of<IUniqueIdentifierGenerator>);
            NcqrsEnvironment.SetGetter<IClock>(Mock.Of<IClock>);
        }

        [Test]
        public void CloneGroupWithoutChildren_When_Group_Have_Condition_With_Variable_Name_Reference_To_Existing_Question_Then_DomainException_should_NOT_be_thrown()
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
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId);
            string expression = string.Format("[{0}] > 0", validatedQuestion.ToString());

            RegisterExpressionProcessorMock(expression, new[] { validatedQuestion.ToString() });

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
        public void CloneGroupWithoutChildren_When_Group_Have_Condition_With_Reference_To_Not_Existing_Question_Then_DomainException_should_be_thrown()
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
        public void CloneGroupWithoutChildren_When_Group_Have_Condition_With_2_References_And_Second_Reference_To_Not_Existing_Question_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId);
            Guid questionId1 = Guid.NewGuid();
            Guid questionId2 = Guid.NewGuid();
            Guid idForNotExistingQuestion = Guid.NewGuid();
            string idForNotExistingQuestionAsString = idForNotExistingQuestion.ToString();

            string expression = string.Format("[{0}] > 0 AND [{1}] > 1", questionId1, questionId2);

            RegisterExpressionProcessorMock(expression, new[] { questionId1.ToString(), idForNotExistingQuestionAsString });

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
