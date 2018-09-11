using System;
using System.Collections.Generic;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerInterviewAccessorTests
{
    [TestOf(typeof(InterviewerInterviewAccessor))]
    internal class when_GetInterviewDuplicatePackageCheck
    {
        readonly Guid interviewId = Id.gA;
        readonly (Guid eventId, DateTime timeStamp) firstEvent = (Id.g1, DateTime.Now);
        readonly (Guid eventId, DateTime timeStamp) lastEvent = (Id.g2, DateTime.UtcNow);
        private List<CommittedEvent> events = null;
        private InterviewerInterviewAccessor subject;

        [SetUp]
        public void Setup()
        {
            var eventStore = new Mock<IEnumeratorEventStorage>();
            eventStore.Setup(e => e.GetPendingEvents(interviewId)).Returns(() => events);

            var optimizer = new Mock<IInterviewEventStreamOptimizer>();
            optimizer
                .Setup(ieso => ieso.RemoveEventsNotNeededToBeSent(It.IsAny<IReadOnlyCollection<CommittedEvent>>()))
                .Returns(() => events.ToReadOnlyCollection());

            subject = Create.Service.InterviewerInterviewAccessor(
                eventStore: eventStore.Object,
                eventStreamOptimizer: optimizer.Object);
        }

        [Test]
        public void should_return_first_last_events_check_info()
        {
            events = new List<CommittedEvent>
            {
                new CommittedEvent(Guid.NewGuid(), "origin", firstEvent.eventId, interviewId, 0, firstEvent.timeStamp, 1, null),
                new CommittedEvent(Guid.NewGuid(), "origin", lastEvent.eventId, interviewId, 1, lastEvent.timeStamp, 2, null)
            };

            var check = subject.GetInterviewEventStreamCheckData(interviewId);

            Assert.That(check.FirstEventId, Is.EqualTo(firstEvent.eventId));
            Assert.That(check.LastEventId, Is.EqualTo(lastEvent.eventId));
            Assert.That(check.FirstEventTimeStamp, Is.EqualTo(firstEvent.timeStamp));
            Assert.That(check.LastEventTimeStamp, Is.EqualTo(lastEvent.timeStamp));
        }

        [Test]
        public void should_return_null_if_no_events_exists()
        {
            events = new List<CommittedEvent>();

            var check = subject.GetInterviewEventStreamCheckData(interviewId);

            Assert.Null(check);
        }
    }
}
