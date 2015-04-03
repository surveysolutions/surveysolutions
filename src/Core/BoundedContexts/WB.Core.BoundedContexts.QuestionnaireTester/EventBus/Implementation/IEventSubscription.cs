using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.EventBus.Implementation
{
    public interface IEventSubscription
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