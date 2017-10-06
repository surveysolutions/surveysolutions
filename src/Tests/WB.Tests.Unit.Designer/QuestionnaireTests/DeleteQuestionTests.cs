using System;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [TestFixture]
    internal class DeleteQuestionTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void DeleteQuestion_When_question_id_specified_Then_raised_QuestionDeleted_event_with_same_question_id()
        {
            // arrange
            Guid questionId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneQuestion(questionId: questionId, responsibleId: responsibleId);

            // act
            questionnaire.DeleteQuestion(questionId, responsibleId);

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId), Is.Null);
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
    }
}