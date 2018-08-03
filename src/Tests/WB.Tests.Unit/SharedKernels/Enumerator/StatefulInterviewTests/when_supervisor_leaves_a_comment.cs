using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [TestOf(typeof(StatefulInterview))]
    public class when_supervisor_leaves_a_comment
    {
        [Test]
        public void when_supervisor_leaves_a_comment_on_hidden_question_should_store_it_and_return_right_counter()
        {
            Guid questionid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion(questionid, scope: QuestionScope.Hidden));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire.PublicKey, userId: Guid.NewGuid(), questionnaire: questionnaire);

            interview.AssignInterviewer(supervisorId, Guid.NewGuid(), DateTime.Now);
            interview.CommentAnswer(supervisorId, questionid, RosterVector.Empty, DateTime.Now, "in comment");

            var commentedBySupervisorQuestionsInInterview = interview.GetCommentedBySupervisorAllQuestions();
            Assert.That(commentedBySupervisorQuestionsInInterview.Count(), Is.EqualTo(1));

            var questionComments = interview.GetQuestionComments(Create.Entity.Identity(questionid, RosterVector.Empty));
            Assert.That(questionComments, Has.Count.EqualTo(1));
        }

        [Test]
        public void when_supervisor_leaves_a_comment_on_supervisor_question_should_store_it_and_return_right_counter()
        {
            Guid questionid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionid, scope: QuestionScope.Supervisor));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            interview.AssignInterviewer(supervisorId, Guid.NewGuid(), DateTime.Now);
            interview.CommentAnswer(supervisorId, questionid, RosterVector.Empty, DateTime.Now, "in comment");

            var commentedBySupervisorQuestionsInInterview = interview.GetCommentedBySupervisorAllQuestions();
            Assert.That(commentedBySupervisorQuestionsInInterview.Count(), Is.EqualTo(1));

            var questionComments = interview.GetQuestionComments(Create.Entity.Identity(questionid, RosterVector.Empty));
            Assert.That(questionComments, Has.Count.EqualTo(1));
        }

        [Test]
        public void when_supervisor_leaves_a_comment_on_prefilled_question_should_store_it_and_return_right_counter()
        {
            Guid questionid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionid, preFilled: true));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            interview.AssignInterviewer(supervisorId, Guid.NewGuid(), DateTime.Now);
            interview.CommentAnswer(supervisorId, questionid, RosterVector.Empty, DateTime.Now, "in comment");

            var commentedBySupervisorQuestionsInInterview = interview.GetCommentedBySupervisorAllQuestions();
            Assert.That(commentedBySupervisorQuestionsInInterview.Count(), Is.EqualTo(1));

            var questionComments = interview.GetQuestionComments(Create.Entity.Identity(questionid, RosterVector.Empty));
            Assert.That(questionComments, Has.Count.EqualTo(1));
        }
    }
}
