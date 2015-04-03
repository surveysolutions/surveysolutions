using System;

namespace WB.Core.Infrastructure.EventBus
{
    public interface IEventBusEventHandler<TEvent> 
    {
        void Handle(TEvent @event);
    }
}