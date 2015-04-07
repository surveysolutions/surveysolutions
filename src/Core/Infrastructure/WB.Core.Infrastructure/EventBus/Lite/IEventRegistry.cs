using System;
using System.Collections.Generic;


namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface IEventRegistry
    {
        void Subscribe(IEventBusEventHandler handler);

        void Unsubscribe(IEventBusEventHandler obj);

        IEnumerable<Action<object>> GetHandlers(object @event);
    }
}