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
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests.Synchronization
{
    internal class when_rejecting_interview_from_HQ : InterviewTestsContext
    {
        Establish context = () =>
        {
            interviewSynchronizationDto = new InterviewSynchronizationDto();
            interviewSynchronizationDto.Status = InterviewStatus.RejectedByHeadquarters;
            commentedQuestionId = Guid.NewGuid();
            interviewSynchronizationDto.Answers = new[]
            {
                new AnsweredQuestionSynchronizationDto(commentedQuestionId, new decimal[] { }, "answer", "answer comment")
            };

            rejectComment = "reject comment";
            synchronizationTime = DateTime.Now;

            var questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<Guid>()))
                .Returns(Mock.Of<IQuestionnaire>());

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(questionnaireRepositoryMock.Object);

            interview = CreateInterview();

            eventContext = new EventContext();
        };

        Because of = () => interview.RejectInterviewFromHeadquarters(userId, Guid.NewGuid(), interviewerId, interviewSynchronizationDto, synchronizationTime, rejectComment);

        It should_raise_InterviewRejectedByHQ_event = () => eventContext.ShouldContainEvent<InterviewRejectedByHQ>(@event => @event.UserId == userId);
        
        It should_reassign_interview_to_interviewer = () => eventContext.ShouldContainEvent<InterviewerAssigned>(@event => @event.InterviewerId == interviewerId);

        It should_put_interivew_in_status_provided_by_HQ = () => eventContext.ShouldContainEvent<InterviewStatusChanged>(@event => 
            @event.Status == interviewSynchronizationDto.Status &&
            @event.Comment == rejectComment);

        It should_apply_answer_comments = () => eventContext.ShouldContainEvent<AnswerCommented>(@event => 
            @event.UserId == userId &&
            @event.Comment == "answer comment" &&
            @event.QuestionId == commentedQuestionId &&
            @event.CommentTime == synchronizationTime // bug here. We don't have comment date in syncrhonization dto, so we cant put correct comment date [KP-3401]
        );

        static Interview interview;
        static EventContext eventContext;
        static Guid userId = Guid.NewGuid();
        private static Guid interviewerId = Guid.NewGuid();
        private static InterviewSynchronizationDto interviewSynchronizationDto ;
        private static string rejectComment;
        private static Guid commentedQuestionId;
        private static DateTime synchronizationTime ;
    }
}