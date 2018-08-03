using System.Collections.Generic;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.InterviewEventStreamOptimizerTests
{
    public class when_interview_received_in_rejected_by_hq_status_and_has_no_completion_event
    {
        [Test]
        public void should_be_able_to_send_it_on_hq()
        {
            var userId = Id.gA;
            var eventStream = new[]
            {
                Create.Other.CommittedEvent(payload: Create.Event.InterviewApproved(userId)),
                Create.Other.CommittedEvent(payload: Create.Event.InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor))
            };

            var optimizer = Create.Service.InterviewEventStreamOptimizer();

            IReadOnlyCollection<CommittedEvent> tobeSent = optimizer.RemoveEventsNotNeededToBeSent(eventStream);
            Assert.That(tobeSent.Count, Is.EqualTo(2));
        }
    }
}
