using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;

namespace WB.Core.Infrastructure.FunctionalDenormalization
{
    public class EventHandlerWrapper
    {
        public EventHandlerWrapper(IEventHandler handler, InProcessEventBus bus)
        {
            this.Handler = handler;
            this.Bus = bus;
            this.Enabled = true;
        }

        public IEventHandler Handler { get; private set; }
        public InProcessEventBus Bus { get; private set; }
        public bool Enabled { get; private set; }

        public void EnableBus()
        {
            this.Enabled = true;
        }

        public void DisableBus()
        {
            this.Enabled = false;
        }
    }
}
