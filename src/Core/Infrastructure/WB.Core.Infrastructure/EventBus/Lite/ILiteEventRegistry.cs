using System;
using System.Collections.Generic;


namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventRegistry
    {
        void Subscribe(ILiteEventHandler handler);

        void Unsubscribe(ILiteEventHandler handler);

        IEnumerable<Action<object>> GetHandlers(object @event);
    }
}