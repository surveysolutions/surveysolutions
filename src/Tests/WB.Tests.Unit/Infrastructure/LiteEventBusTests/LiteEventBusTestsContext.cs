using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    [Subject(typeof(LiteEventBus))]
    public class LiteEventBusTestsContext
    {
        public class DumyLiteEventHandlers : ILiteEventHandler<int>, ILiteEventHandler<long>, ILiteEventHandler<string>
        {
            public virtual void Handle(int @event) { }

            public virtual void Handle(long @event) { }

            public virtual void Handle(string @event) { }
        }

        public class DummyEvent
        {
            public object Obj { get; set; } 
        }

        public static DummyEvent CreateDummyEvent()
        {
            return new DummyEvent();
        }         
        
        public static IAggregateRoot CreateDummyAggregateRoot(params object[] events)
        {
            UncommittedEvent[] uncommittedEvents = events.Select(CreateUncommitedEvent).ToArray();
            Mock<IAggregateRoot> mock = new Mock<IAggregateRoot>();
            mock.Setup(a => a.GetUncommittedChanges()).Returns(uncommittedEvents);
            return mock.Object;
        }

        private static UncommittedEvent CreateUncommitedEvent(object @event)
        {
            //return e => Mock.Of<UncommittedEvent>(m => m.Payload == e);
            return new UncommittedEvent(Guid.Empty, Guid.Empty, 0, 0, DateTime.Now, @event);
        }

        public static DumyLiteEventHandlers CreateDummyClassWithEventHandlers() 
        {
            return Mock.Of<DumyLiteEventHandlers>();
        }  
    }
}