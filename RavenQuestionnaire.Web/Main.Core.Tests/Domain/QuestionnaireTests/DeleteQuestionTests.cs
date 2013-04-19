using System;
using Main.Core.Domain;
using Main.Core.Events.Questionnaire;
using NUnit.Framework;
using Ncqrs.Spec;

namespace Main.Core.Tests.Domain.QuestionnaireTests
{
    [TestFixture]
    public class DeleteQuestionTests : QuestionnaireARTestContext
    {
        [Test]
        public void DeleteQuestion_When_question_id_specified_Then_raised_QuestionDeleted_event_with_same_question_id()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid questionId = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneQuestion(questionId);

                // act
                Guid parentPublicKey = Guid.NewGuid();
                questionnaire.NewDeleteQuestion(questionId);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<QuestionDeleted>(eventContext).QuestionId, Is.EqualTo(questionId));
            }
        }
    }
}