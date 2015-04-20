using System;
using System.Collections.Generic;


namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventRegistry
    {
        void Subscribe(ILiteEventBusEventHandler handler);

        void Unsubscribe(ILiteEventBusEventHandler handler);

        IEnumerable<Action<object>> GetHandlers(object @event);
    }
}