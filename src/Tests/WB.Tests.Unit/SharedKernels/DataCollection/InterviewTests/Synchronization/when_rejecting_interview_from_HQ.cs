using System;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    internal class when_rejecting_interview_from_HQ : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rejectComment = "reject comment";
            interviewSynchronizationDto = new InterviewSynchronizationDto { Status = InterviewStatus.RejectedByHeadquarters, Comments = rejectComment};
            commentedQuestionId = Guid.NewGuid();
            answerComment = new CommentSynchronizationDto
            {
                Date = new DateTime(2010, 1, 1),
                Text = "Comment text",
                UserId = Guid.NewGuid()
            };
            interviewSynchronizationDto.Answers = new[]
            {
                Create.Entity.AnsweredQuestionSynchronizationDto(commentedQuestionId, new decimal[] { }, "answer",
                    comments: new[] {answerComment})
            };

            synchronizationTime = DateTimeOffset.Now;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion(commentedQuestionId));

            interview = Setup.StatefulInterview(questionnaire);

            interview.AssignInterviewer(supervisorId, userId, DateTimeOffset.Now);
            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.Completed));
            interview.Approve(userId, string.Empty, DateTimeOffset.Now);

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() => interview.RejectInterviewFromHeadquarters(userId, supervisorId, interviewerId, interviewSynchronizationDto, synchronizationTime);

        [NUnit.Framework.Test] public void should_raise_InterviewRejectedByHQ_event () => eventContext.ShouldContainEvent<InterviewRejectedByHQ>(@event => @event.UserId == userId);
        
        [NUnit.Framework.Test] public void should_reassign_interview_to_interviewer () => eventContext.ShouldContainEvent<InterviewerAssigned>(@event => @event.InterviewerId == interviewerId);

        [NUnit.Framework.Test] public void should_put_interivew_in_status_provided_by_HQ () => eventContext.ShouldContainEvent<InterviewStatusChanged>(@event => 
            @event.Status == interviewSynchronizationDto.Status &&
            @event.Comment == rejectComment);

        [NUnit.Framework.Test] public void should_apply_answer_comments () =>  eventContext.ShouldContainEvent<AnswerCommented>(@event => 
            @event.UserId == answerComment.UserId &&
            @event.Comment == answerComment.Text &&
            @event.QuestionId == commentedQuestionId &&
            @event.OriginDate == answerComment.Date
        );

        static Interview interview;
        static EventContext eventContext;
        static Guid userId = Guid.NewGuid();
        static Guid supervisorId = Guid.NewGuid();
        private static Guid interviewerId = Guid.NewGuid();
        private static InterviewSynchronizationDto interviewSynchronizationDto ;
        private static string rejectComment;
        private static Guid commentedQuestionId;
        private static DateTimeOffset synchronizationTime ;
        private static CommentSynchronizationDto answerComment;
    }
}
