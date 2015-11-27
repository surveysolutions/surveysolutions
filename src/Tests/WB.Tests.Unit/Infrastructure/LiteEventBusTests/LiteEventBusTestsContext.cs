using System;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    public class LiteEventBusTestsContext
    {
        public class DummyEvent : IEvent  { }

        public class DifferentDummyEvent : IEvent { }

        internal static DummyEvent CreateDummyEvent()
        {
            return new DummyEvent();
        }

        protected static CommittedEventStream BuildReadyToBePublishedStream(Guid eventSourceId, IEvent @event)
        {
            return new CommittedEventStream(eventSourceId,
                Create.CommittedEvent(eventSourceId: eventSourceId, payload: @event));
        }
    }
}