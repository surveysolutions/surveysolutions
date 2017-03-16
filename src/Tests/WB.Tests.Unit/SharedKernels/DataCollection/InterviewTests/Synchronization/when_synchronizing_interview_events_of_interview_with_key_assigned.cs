using System;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    [TestFixture]
    [TestOf(typeof(Interview))]
    public class when_synchronizing_interview_events_of_interview_with_key_assigned
    {
        private EventContext eventContext = null;
        private readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Test]
        public void Should_not_change_interview_id()
        {
            var interview = Create.AggregateRoot.Interview();
            interview.Apply(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, ""));
            interview.Apply(new InterviewerAssigned(userId, userId, DateTime.Now));
            interview.Apply(new InterviewKeyAssigned(Create.Entity.InterviewKey(6)));

            var command = Create.Command.SynchronizeInterviewEventsCommand(interviewKey: Create.Entity.InterviewKey(8));

            this.eventContext = new EventContext();
            interview.SynchronizeInterviewEvents(command);

            this.eventContext.ShouldNotContainEvent<InterviewKeyAssigned>();
        }

        [TearDown]
        public void TearDown()
        {
            this.eventContext?.Dispose();
            this.eventContext = null;
        }
    }
}