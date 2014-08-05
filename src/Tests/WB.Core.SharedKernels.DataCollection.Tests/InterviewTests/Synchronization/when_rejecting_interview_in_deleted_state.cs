using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests.Synchronization
{
    internal class when_rejecting_interview_from_HQ_in_deleted_state : InterviewTestsContext
    {
        Establish context = () =>
        {
            interview = CreateInterview();
            interview.Apply(new InterviewStatusChanged(InterviewStatus.Deleted, null));
            eventContext = new EventContext();
        };

        Because of = () => interview.RejectInterviewFromHeadquarters(userId, Guid.NewGuid(), Guid.NewGuid(), new InterviewSynchronizationDto(), DateTime.Now);

        It should_restore_interview = () => eventContext.ShouldContainEvent<InterviewRestored>(@event => @event.UserId == userId);

        static Interview interview;
        static EventContext eventContext;
        static Guid userId = Guid.NewGuid();
    }
}