using System;

namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    public interface IEventSubscription : IDisposable
    {
        void RaiseEvent(object @event);
    }

    public interface IEventSubscription<TEvent> : IEventSubscription
    {
        void Subscribe(Action<TEvent> handler);

        void Unsubscribe(Action<TEvent> handler);

        void RaiseEvent(TEvent @event);
    }
}