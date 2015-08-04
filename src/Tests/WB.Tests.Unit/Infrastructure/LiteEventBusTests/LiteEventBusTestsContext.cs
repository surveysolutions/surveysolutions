using System;
using System.Linq;
using Moq;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;


namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    public class LiteEventBusTestsContext
    {
        public class DummyEvent  { }

        public class DifferentDummyEvent  { }

        public static IAggregateRoot CreateDummyAggregateRoot(Guid eventSourceId, params object[] events)
        {
            UncommittedEvent[] uncommittedEvents = events.Select(o => CreateUncommitedEvent(o, eventSourceId)).ToArray();
            Mock<IAggregateRoot> mock = new Mock<IAggregateRoot>();
            mock.Setup(a => a.GetUncommittedChanges()).Returns(uncommittedEvents);
            mock.Setup(x => x.EventSourceId).Returns(eventSourceId);
            return mock.Object;
        }

        private static UncommittedEvent CreateUncommitedEvent(object @event, Guid eventSourceId)
        {
            return new UncommittedEvent(Guid.Empty, eventSourceId, 0, 0, DateTime.Now, @event);
        }

        internal static DummyEvent CreateDummyEvent()
        {
            return new DummyEvent();
        }

        protected static IAggregateRoot SetupAggregateRootWithEventReadyForPublishing<T>(Guid eventSourceId, T @event)
        {
            return CreateDummyAggregateRoot(eventSourceId, @event);
        }

        protected static IAggregateRoot SetupAggregateRootWithOneEventReadyForPublishing<T>() where T:new()
        {
            T obj = new T();
            return CreateDummyAggregateRoot(Guid.NewGuid(), obj);
        }
    }
}