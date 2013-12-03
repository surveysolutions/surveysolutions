using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.FunctionalDenormalization
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
