﻿using System;
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
    public class DeleteQuestionTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void DeleteQuestion_When_question_id_specified_Then_raised_QuestionDeleted_event_with_same_question_id()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid questionId = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneQuestion(questionId: questionId, responsibleId: responsibleId);

                // act
                questionnaire.DeleteQuestion(questionId, responsibleId);

                // assert
                Assert.That(GetSingleEvent<QuestionDeleted>(eventContext).QuestionId, Is.EqualTo(questionId));
            }
        }

        [Test]
        public void DeleteQuestion_When_User_Doesnot_Have_Permissions_For_Edit_Questionnaire_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid questionId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneQuestion(questionId: questionId, responsibleId: Guid.NewGuid());

            // act
            TestDelegate act = () => questionnaire.DeleteQuestion(questionId, Guid.NewGuid());
            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

        [Test]
        public void DeleteQuestion_When_Question_Is_not_involved_in_the_validations_and_conditions_of_other_questions_Then_DomainException_should_NOT_be_thrown()
        {
            // arrange
            Guid question1Id = Guid.NewGuid();
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId);

            AddQuestion(questionnaire, question1Id, groupId, responsibleId, QuestionType.Text, "q1");

            // act
            TestDelegate act = () => questionnaire.DeleteQuestion(question1Id, responsibleId);

            // assert
            Assert.DoesNotThrow(act);
        }

        [Test]
        public void DeleteQuestion_When_Question_Variable_involved_in_the_condition_of_other_question_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid question1Id = Guid.NewGuid();
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId);

            string aliasForDeletedQuestion = "q1";
            string expression = string.Format("[{0}] > 0", aliasForDeletedQuestion);

            RegisterExpressionProcessorMock(expression, new[] { aliasForDeletedQuestion });
            
            AddQuestion(questionnaire, question1Id, groupId, responsibleId, QuestionType.Text, aliasForDeletedQuestion);
            AddQuestion(questionnaire, Guid.NewGuid(), groupId, responsibleId, QuestionType.Text, "q2",
                condition: expression);


            // act
            TestDelegate act = () => questionnaire.DeleteQuestion(question1Id, responsibleId);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionOrGroupDependOnAnotherQuestion));
        }

        [Test]
        public void DeleteQuestion_When_Question_Id_involved_in_the_condition_of_other_question_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid question1Id = Guid.NewGuid();
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId);

            string expression = string.Format("[{0}] > 0", question1Id);

            RegisterExpressionProcessorMock(expression, new[] { question1Id.ToString() });

            AddQuestion(questionnaire, question1Id, groupId, responsibleId, QuestionType.Text, "q1");
            AddQuestion(questionnaire, Guid.NewGuid(), groupId, responsibleId, QuestionType.Text, "q2",
                condition: expression);


            // act
            TestDelegate act = () => questionnaire.DeleteQuestion(question1Id, responsibleId);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionOrGroupDependOnAnotherQuestion));
        }

        [Test]
        public void DeleteQuestion_When_Question_Variable_involved_in_the_validation_of_other_question_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid question1Id = Guid.NewGuid();
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId);
            string aliasForDeletedQuestion = "q1";
            string expression = string.Format("[{0}] > 0", aliasForDeletedQuestion);

            RegisterExpressionProcessorMock(expression, new[] { aliasForDeletedQuestion });

            AddQuestion(questionnaire, question1Id, groupId, responsibleId, QuestionType.Text, aliasForDeletedQuestion);
            AddQuestion(questionnaire, Guid.NewGuid(), groupId, responsibleId, QuestionType.Text, "q2",
                validation: expression);

            // act
            TestDelegate act = () => questionnaire.DeleteQuestion(question1Id, responsibleId);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionOrGroupDependOnAnotherQuestion));
        }

        [Test]
        public void DeleteQuestion_When_Question_Id_involved_in_the_validation_of_other_question_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid question1Id = Guid.NewGuid();
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: groupId);

            string expression = string.Format("[{0}] > 0", question1Id);

            RegisterExpressionProcessorMock(expression, new[] { question1Id.ToString() });

            AddQuestion(questionnaire, question1Id, groupId, responsibleId, QuestionType.Text, "q1");
            AddQuestion(questionnaire, Guid.NewGuid(), groupId, responsibleId, QuestionType.Text, "q2",
                validation: expression);

            // act
            TestDelegate act = () => questionnaire.DeleteQuestion(question1Id, responsibleId);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionOrGroupDependOnAnotherQuestion));
        }
    }
}