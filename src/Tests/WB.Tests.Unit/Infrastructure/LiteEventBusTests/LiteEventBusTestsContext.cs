using System;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    public class LiteEventBusTestsContext
    {
        public class DummyEvent : ILiteEvent  { }

        public class DifferentDummyEvent : ILiteEvent { }

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