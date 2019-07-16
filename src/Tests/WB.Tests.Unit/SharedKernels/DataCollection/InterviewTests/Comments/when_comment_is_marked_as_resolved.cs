using System;
using System.Linq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Comments
{
    public class when_comment_is_marked_as_resolved
    {
        [Test]
        public void when_no_comment_exists_Should_throw_exception()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: Create.Entity.TextQuestion(Id.gA));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            // Act
            TestDelegate act = () => interview.ResolveComment(Create.Command.ResolveCommentAnswer(interview.Id, Create.Identity(Id.gA)));

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<InterviewException>().With.Message.EqualTo("All question comments are already resolved"));
        }

        [Test]
        public void should_mark_as_resolved()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: Create.Entity.TextQuestion(Id.gA));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            Guid commentId;
            using (var eventContext = new EventContext())
            {
                interview.CommentAnswer(Guid.NewGuid(), Id.gA, RosterVector.Empty, DateTimeOffset.UtcNow, "comment");
                commentId = eventContext.GetEvent<AnswerCommented>().CommentId.GetValueOrDefault();
            }

            // Act
            var entityId = Create.Identity(Id.gA);
            interview.ResolveComment(Create.Command.ResolveCommentAnswer(interview.Id, entityId));

            // Assert
            var interviewTreeQuestion = interview.GetQuestion(entityId);
            var answerComment = interviewTreeQuestion.AnswerComments.First(x => x.Id == commentId);
            Assert.That(answerComment, Has.Property(nameof(AnswerComment.Resolved)).True);
        }
    }
}
