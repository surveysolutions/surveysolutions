using System;
using System.Linq;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;

namespace WB.Tests.Unit.Infrastructure.EventBusMobileTests
{
    public class EventBusTestsContext
    {
        public class DumyEventHandlers : IEventBusEventHandler<int>, IEventBusEventHandler<long>, IEventBusEventHandler<string>
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

        public static DumyEventHandlers CreateDummyClassWithEventHandlers() 
        {
            return Mock.Of<DumyEventHandlers>();
        }  
        
        public static IEventRegistry CreateEventRegistry()
        {
            return new EventRegistry();
        }

        public static ILiteEventBus CreateEventBus(IEventRegistry eventRegistry = null)
        {
            var eventReg = eventRegistry ?? Mock.Of<IEventRegistry>();
            return new LiteEventBus(eventReg);
        }
    }
}