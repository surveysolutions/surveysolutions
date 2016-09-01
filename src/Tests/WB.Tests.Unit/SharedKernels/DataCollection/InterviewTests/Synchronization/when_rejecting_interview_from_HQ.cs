using System;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    internal class when_rejecting_interview_from_HQ : InterviewTestsContext
    {
        Establish context = () =>
        {
            rejectComment = "reject comment";
            interviewSynchronizationDto = new InterviewSynchronizationDto { Status = InterviewStatus.RejectedByHeadquarters, Comments = rejectComment};
            commentedQuestionId = Guid.NewGuid();
            answerComment = new CommentSynchronizationDto
            {
                Date = new DateTime(2010, 1, 1),
                Text ="Comment text",
                UserId = Guid.NewGuid()
            };
            interviewSynchronizationDto.Answers = new[]
            {
                Create.Entity.AnsweredQuestionSynchronizationDto(commentedQuestionId, new decimal[] { }, "answer", comments: new CommentSynchronizationDto[] { answerComment })
            };

            synchronizationTime = DateTime.Now;

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), _ => true);

            interview = CreateInterview(questionnaireRepository: questionnaireRepository);

            interview.AssignInterviewer(supervisorId, userId, DateTime.Now);
            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.Completed));
            interview.Approve(userId, string.Empty, DateTime.Now);

            eventContext = new EventContext();
        };

        Because of = () => interview.RejectInterviewFromHeadquarters(userId, supervisorId, interviewerId, interviewSynchronizationDto, synchronizationTime);

        It should_raise_InterviewRejectedByHQ_event = () => eventContext.ShouldContainEvent<InterviewRejectedByHQ>(@event => @event.UserId == userId);
        
        It should_reassign_interview_to_interviewer = () => eventContext.ShouldContainEvent<InterviewerAssigned>(@event => @event.InterviewerId == interviewerId);

        It should_put_interivew_in_status_provided_by_HQ = () => eventContext.ShouldContainEvent<InterviewStatusChanged>(@event => 
            @event.Status == interviewSynchronizationDto.Status &&
            @event.Comment == rejectComment);

        It should_apply_answer_comments = () => eventContext.ShouldContainEvent<AnswerCommented>(@event => 
            @event.UserId == answerComment.UserId &&
            @event.Comment == answerComment.Text &&
            @event.QuestionId == commentedQuestionId &&
            @event.CommentTime == answerComment.Date
        );

        static Interview interview;
        static EventContext eventContext;
        static Guid userId = Guid.NewGuid();
        static Guid supervisorId = Guid.NewGuid();
        private static Guid interviewerId = Guid.NewGuid();
        private static InterviewSynchronizationDto interviewSynchronizationDto ;
        private static string rejectComment;
        private static Guid commentedQuestionId;
        private static DateTime synchronizationTime ;
        private static CommentSynchronizationDto answerComment;
    }
}