using Ncqrs.Spec;
using System;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    internal class when_rejecting_interview_with_comments_from_HQ : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewSynchronizationDto = new InterviewSynchronizationDto { Status = InterviewStatus.RejectedByHeadquarters };
            commentedQuestionId = Guid.NewGuid();

            existingComment = new CommentSynchronizationDto
            {
                Date = new DateTime(2009, 1, 1),
                Text = "old comment text",
                UserId = userId
            };

            newComment = new CommentSynchronizationDto
            {
                Date = new DateTime(2010, 1, 1),
                Text = "Comment text",
                UserId = userId
            };

            interviewSynchronizationDto.Answers = new[]
            {
                Create.Entity.AnsweredQuestionSynchronizationDto(commentedQuestionId, new decimal[] { }, "answer",
                    comments: new[] {existingComment, newComment})
            };

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion(commentedQuestionId));

            interview = Setup.StatefulInterview(questionnaire);
            interview.Apply(new AnswerCommented(userId, commentedQuestionId, new decimal[]{}, new DateTimeOffset(existingComment.Date, new TimeSpan(0)), existingComment.Text));

            interview.AssignInterviewer(supervisorId, userId, DateTimeOffset.Now);
            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.Completed));
            interview.Approve(userId, string.Empty, DateTimeOffset.Now);

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() => interview.RejectInterviewFromHeadquarters(userId, supervisorId, interviewerId, interviewSynchronizationDto, DateTimeOffset.Now);


        [NUnit.Framework.Test] public void should_add_new_comments () => eventContext.ShouldContainEvent<AnswerCommented>(@event => 
            @event.Comment == newComment.Text 
        );

        [NUnit.Framework.Test] public void should_not_add_alrady_saved_comments () => eventContext.ShouldNotContainEvent<AnswerCommented>(@event => @event.Comment == existingComment.Text);

        static Interview interview;
        static EventContext eventContext;
        static Guid userId = Guid.NewGuid();
        static Guid supervisorId = Guid.NewGuid();
        private static Guid interviewerId = Guid.NewGuid();
        private static InterviewSynchronizationDto interviewSynchronizationDto ;
        private static Guid commentedQuestionId;
        private static CommentSynchronizationDto newComment;
        private static CommentSynchronizationDto existingComment;
    }
}
