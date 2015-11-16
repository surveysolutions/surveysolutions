using System;
using Ncqrs.Eventing;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    public class LiteEventBusTestsContext
    {
        public class DummyEvent  { }

        public class DifferentDummyEvent { }

        internal static DummyEvent CreateDummyEvent()
        {
            return new DummyEvent();
        }

        protected static CommittedEventStream BuildReadyToBePublishedStream(Guid eventSourceId, object @event)
        {
            return new CommittedEventStream(eventSourceId,
                Create.CommittedEvent(eventSourceId: eventSourceId, payload: @event));
        }
    }
}