using System;
using System.Collections.Generic;
using Ncqrs.Eventing;


namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventRegistry
    {
        void Subscribe(ILiteEventHandler handler, string aggregateRootId);

        void Unsubscribe(ILiteEventHandler handler, string aggregateRootId);

        bool IsSubscribed(ILiteEventHandler handler, string eventSourceId);

        IReadOnlyCollection<Action<object>> GetHandlers(CommittedEvent @event);
    }
}