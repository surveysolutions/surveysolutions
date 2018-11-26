using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.Infrastructure.Implementation.EventDispatcher
{
    public class EventHandlerWrapper
    {
        public EventHandlerWrapper(Type handler, InProcessEventBus bus)
        {
            this.Handler = handler;
            this.Bus = bus;
        }

        public Type Handler { get; private set; }
        public InProcessEventBus Bus { get; private set; }
    }
}
