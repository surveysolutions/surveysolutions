using System;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_count_of_enabled_questions_and_interview_has_prefilled_question : StatefulInterviewTestsContext
    {
        [Test]
        public void should_count_prefilled_question()
        {
            //arrange
            var prefilledQuestionId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId, variable: "q1"), 
                Create.Entity.TextQuestion(prefilledQuestionId, variable: "q2", preFilled: true));

            var statefulInterview = Setup.StatefulInterview(questionnaire);
            //act
            var countActiveQuestionsInInterview = statefulInterview.CountActiveQuestionsInInterview();
            //assert
            Assert.That(countActiveQuestionsInInterview, Is.EqualTo(2));
        }
    }
}