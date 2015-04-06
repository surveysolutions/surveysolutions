using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils.Services;
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

        public static IPublishableEvent CreatePublishableEvent()
        {
            return CreateEvent<IPublishableEvent>();
        }         
        
        public static TEvent CreateEvent<TEvent>() where TEvent : class
        {
            return Mock.Of<TEvent>();
        }

        public static DumyEventHandlers CreateClassWithEventHandlers() 
        {
            return Mock.Of<DumyEventHandlers>();
        }  
        
        public static IEventRegistry CreateEventRegistry()
        {
            var logger = Mock.Of<ILogger>();
            return new EventRegistry(logger);
        }

        public static ILiteEventBus CreateEventBus(IEventRegistry eventRegistry = null)
        {
            var logger = Mock.Of<ILogger>();
            var eventReg = eventRegistry ?? Mock.Of<IEventRegistry>();
            return new LiteEventBus(logger, eventReg);
        }
    }
}