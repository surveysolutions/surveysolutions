using System;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_count_of_all_enabled_questions_and_census_interview_and_has_prefilled_question : StatefulInterviewTestsContext
    {
        [Test]
        public void should_count_prefilled_question()
        {
            //arrange
            var prefilledQuestionId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Guid.Parse("22222222222222222222222222222222");
            var responsibleId = Guid.Parse("33333333333333333333333333333333");
            var suervisorId = Guid.Parse("44444444444444444444444444444444");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId),
                    Create.Entity.TextQuestion(prefilledQuestionId, preFilled: true)
                });

            var statefulInterview = Setup.StatefulInterview(questionnaire);
            statefulInterview.AnswerTextQuestion(responsibleId, prefilledQuestionId, RosterVector.Empty, DateTime.UtcNow, "prefilled text");
            statefulInterview.AnswerTextQuestion(responsibleId, questionId, RosterVector.Empty, DateTime.UtcNow, "regular text");
            //act
            var countActiveQuestionsInInterview = statefulInterview.CountAllEnabledAnsweredQuestions();
            //assert
            Assert.That(countActiveQuestionsInInterview, Is.EqualTo(2));
        }
    }
}