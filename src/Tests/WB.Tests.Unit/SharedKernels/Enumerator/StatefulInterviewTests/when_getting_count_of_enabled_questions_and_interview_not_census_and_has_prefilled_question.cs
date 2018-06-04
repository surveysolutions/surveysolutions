using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_count_of_enabled_questions_and_interview_not_census_and_has_prefilled_question : StatefulInterviewTestsContext
    {
        [Test]
        public void should_not_count_prefilled_question()
        {
            //arrange
            var prefilledQuestionId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Guid.Parse("22222222222222222222222222222222");
            var responsibleId = Guid.Parse("33333333333333333333333333333333");
            var suervisorId = Guid.Parse("44444444444444444444444444444444");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId, variable: "q1"), 
                Create.Entity.TextQuestion(prefilledQuestionId, variable: "q2", preFilled: true)
            );

            var statefulInterview = Setup.StatefulInterview(questionnaire, false);
            statefulInterview.CreateInterview(
                Create.Command.CreateInterview(questionnaire.PublicKey, 1, suervisorId, new List<InterviewAnswer>(), responsibleId));

            //act
            var countActiveQuestionsInInterview = statefulInterview.CountActiveQuestionsInInterview();
            //assert
            Assert.That(countActiveQuestionsInInterview, Is.EqualTo(2));
        }
    }
}
