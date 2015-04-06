using System;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    class EventSubscription<TEvent> : IEventSubscription<TEvent>
    {
        ImmutableList<Action<TEvent>> eventHandlers = new ImmutableList<Action<TEvent>>();
        private readonly object lockObject = new object();
        private bool isDisposed;

        public void Subscribe(Action<TEvent> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            lock (lockObject)
            {
                EnsureIsNotDisposed();
                eventHandlers = eventHandlers.Add(handler);
            }
        }

        public void Unsubscribe(Action<TEvent> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            lock (lockObject)
            {
                EnsureIsNotDisposed();
                eventHandlers = eventHandlers.Remove(handler);
            }
        }

        public void RaiseEvent(TEvent @event)
        {
            RaiseEvent((object) @event);
        }

        public void RaiseEvent(object @event)
        {
            Action<TEvent>[] actions;
            lock (lockObject)
            {
                EnsureIsNotDisposed();
                actions = eventHandlers.Data;
            }

            if (actions == null || actions.Length == 0)
                return;

            TEvent eventType = (TEvent)@event;
            foreach (Action<TEvent> action in actions)
            {
                FireEventAsync(action, eventType);
            }
        }

        public async void FireEventAsync(Action<TEvent> action, TEvent @event)
        {
            await Task.Run(() => action(@event));
        }

        private void EnsureIsNotDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(string.Empty);
        }

        public void Dispose()
        {
            lock (lockObject)
            {
                isDisposed = true;
                eventHandlers = null;
            }
        }
    }
}