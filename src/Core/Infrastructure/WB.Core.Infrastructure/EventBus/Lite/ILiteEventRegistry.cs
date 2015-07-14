using System;
using System.Collections.Generic;
using Ncqrs.Eventing;


namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventRegistry
    {
        void Subscribe(ILiteEventHandler handler, string eventSourceId);

        void Unsubscribe(ILiteEventHandler handler, string eventSourceId);

        IEnumerable<Action<object>> GetHandlers(UncommittedEvent @event);
    }
}