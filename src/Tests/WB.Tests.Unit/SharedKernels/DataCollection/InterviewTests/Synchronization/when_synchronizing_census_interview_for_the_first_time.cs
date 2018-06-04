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
    internal class when_synchronizing_census_interview_for_the_first_time
    {
        private EventContext eventContext = null;
        private readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Test]
        public void Should_raise_interview_key_assigned_event()
        {
            var interview = Create.AggregateRoot.Interview();
            interview.Apply(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, "", DateTimeOffset.Now));
            interview.Apply(new InterviewerAssigned(userId, userId, DateTimeOffset.Now));

            var command = Create.Command.SynchronizeInterviewEventsCommand(interviewKey: Create.Entity.InterviewKey(8));

            this.eventContext = new EventContext();
            interview.SynchronizeInterviewEvents(command);

            this.eventContext.ShouldContainEvent<InterviewKeyAssigned>(x => x.Key.RawValue == 8);
        }

        [TearDown]
        public void TearDown()
        {
            this.eventContext?.Dispose();
            this.eventContext = null;
        }
    }
}
