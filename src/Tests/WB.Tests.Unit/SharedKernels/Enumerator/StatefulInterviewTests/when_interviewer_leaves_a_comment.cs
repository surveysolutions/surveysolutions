using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [TestOf(typeof(StatefulInterview))]
    public class when_interviewer_leaves_a_comment
    {
        [Test]
        public void Should_store_it_and_return_as_interviewer_comment()
        {
            Guid questionid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion(questionid));

            var interview = Create.AggregateRoot.StatefulInterview(userId: interviewerId,
                questionnaire: Create.Entity.PlainQuestionnaire(questionnaire));

            interview.CommentAnswer(interviewerId, questionid, RosterVector.Empty, DateTime.Now, "in comment");

            var commentedBySupervisorQuestionsInInterview = interview.GetCommentedBySupervisorQuestionsInInterview();
            Assert.That(commentedBySupervisorQuestionsInInterview, Is.Empty);

            var questionComments = interview.GetQuestionComments(Create.Entity.Identity(questionid, RosterVector.Empty));
            Assert.That(questionComments, Has.Count.EqualTo(1));
        }
    }
}