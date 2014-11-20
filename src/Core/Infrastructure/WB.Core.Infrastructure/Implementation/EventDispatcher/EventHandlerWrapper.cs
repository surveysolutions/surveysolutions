using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.Infrastructure.Implementation.EventDispatcher
{
    public class EventHandlerWrapper
    {
        public EventHandlerWrapper(IEventHandler handler, InProcessEventBus bus)
        {
            this.Handler = handler;
            this.Bus = bus;
        }

        public IEventHandler Handler { get; private set; }
        public InProcessEventBus Bus { get; private set; }
    }
}
