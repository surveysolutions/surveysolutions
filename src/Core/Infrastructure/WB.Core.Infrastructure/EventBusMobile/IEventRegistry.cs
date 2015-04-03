using System;
using WB.Core.Infrastructure.EventBus.Implementation;


namespace WB.Core.Infrastructure.EventBus
{
    public interface IEventRegistry
    {
        void Subscribe<TEvent>(Action<TEvent> handler);
        void Subscribe<TEvent>(IEventBusEventHandler<TEvent> handler);
        void Subscribe(object obj);

        void Unsubscribe<TEvent>(Action<TEvent> handler);
        void Unsubscribe<TEvent>(IEventBusEventHandler<TEvent> handler);
        void Unsubscribe(object obj);

        IEventSubscription<TEvent> GetSubscription<TEvent>();
    }
}