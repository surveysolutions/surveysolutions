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

        public static IAggregateRoot CreateDummyAggregateRoot(params object[] events)
        {
            UncommittedEvent[] uncommittedEvents = events.Select(CreateUncommitedEvent).ToArray();
            Mock<IAggregateRoot> mock = new Mock<IAggregateRoot>();
            mock.Setup(a => a.GetUncommittedChanges()).Returns(uncommittedEvents);
            return mock.Object;
        }

        private static UncommittedEvent CreateUncommitedEvent(object @event)
        {
            return new UncommittedEvent(Guid.Empty, Guid.Empty, 0, 0, DateTime.Now, @event);
        }

        internal static DummyEvent CreateDummyEvent()
        {
            return new DummyEvent();
        }

        protected static IAggregateRoot SetupAggregateRootWithEventReadyForPublishing<T>(T @event)
        {
            return CreateDummyAggregateRoot(@event);
        }

        protected static IAggregateRoot SetupAggregateRootWithOneEventReadyForPublishing<T>() where T:new()
        {
            T obj = new T();
            return CreateDummyAggregateRoot(obj);
        }
    }
}