using System;
using System.Linq.Expressions;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_marking_interview_as_sent_to_headquarters_and_interview_is_approved_by_supervisor : InterviewTestsContext
    {
        Establish context = () =>
        {
            SetupInstanceToMockedServiceLocator(new Mock<IQuestionnaireRepository> { DefaultValue = DefaultValue.Mock }.Object);

            interview = CreateInterview();

            interview.Apply(new InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor, comment: null));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.MarkInterviewAsSentToHeadquarters(userId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_InterviewStatusChanged_event = () =>
            eventContext.ShouldContainEvent<InterviewStatusChanged>();

        It should_raise_InterviewStatusChanged_event_with_Deleted_status = () =>
            eventContext.ShouldContainEvent<InterviewStatusChanged>(@event
                => @event.Status == InterviewStatus.Deleted);

        It should_raise_InterviewDeleted_event = () =>
            eventContext.ShouldContainEvent<InterviewDeleted>();

        It should_raise_InterviewSentToHeadquarters_event = () =>
            eventContext.ShouldContainEvent<InterviewSentToHeadquarters>();

        private static Interview interview;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static EventContext eventContext;
    }
}