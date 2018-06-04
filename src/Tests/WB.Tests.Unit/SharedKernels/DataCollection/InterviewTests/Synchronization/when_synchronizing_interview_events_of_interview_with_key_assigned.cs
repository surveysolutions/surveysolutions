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
        private readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");

        [Test]
        public void Should_not_change_interview_id()
        {
            var interview = Create.AggregateRoot.Interview();
            interview.Apply(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, "", DateTimeOffset.Now));
            interview.Apply(new InterviewerAssigned(userId, userId, DateTimeOffset.Now));
            interview.Apply(new InterviewKeyAssigned(Create.Entity.InterviewKey(6), DateTimeOffset.Now));

            var newKey = Create.Entity.InterviewKey(8);
            var command = Create.Command.SynchronizeInterviewEventsCommand(
                interviewKey: newKey,
                questionnaireId: questionnaireId,
                questionnaireVersion: 1);

            this.eventContext = new EventContext();
            interview.SynchronizeInterviewEvents(command);

            this.eventContext.ShouldContainEvent<InterviewKeyAssigned>(x => x.Key == newKey);
        }

        [TearDown]
        public void TearDown()
        {
            this.eventContext?.Dispose();
            this.eventContext = null;
        }
    }
}
