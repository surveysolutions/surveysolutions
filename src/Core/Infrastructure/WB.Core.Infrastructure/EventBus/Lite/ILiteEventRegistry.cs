using System;
using System.Collections.Generic;
using Ncqrs.Eventing;


namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventRegistry
    {
        void Subscribe(ILiteEventHandler handler, string aggregateRootId = null);

        void Unsubscribe(ILiteEventHandler handler, string aggregateRootId = null);

        bool IsSubscribed(ILiteEventHandler handler, string aggregateRootId = null);

        IReadOnlyCollection<Action<object>> GetHandlers(CommittedEvent @event);
    }
}