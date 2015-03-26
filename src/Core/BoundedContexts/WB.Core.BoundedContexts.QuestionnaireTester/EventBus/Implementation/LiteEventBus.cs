using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.BoundedContexts.QuestionnaireTester.EventBus.Implementation
{
    public class LiteEventBus : IEventBus, IEventRegistry
    {
        private readonly ILogger logger;

        private readonly ConcurrentDictionary<Type, EventSubscription> subscriptions = new ConcurrentDictionary<Type, EventSubscription>();

        public LiteEventBus(ILogger logger)
        {
            this.logger = logger;
        }

        #region IMessageBus Members

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var subscription = (EventSubscription<TEvent>)subscriptions.GetOrAdd(
                typeof(TEvent),
                t => new EventSubscription<TEvent>());
            subscription.Subscribe(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof (TEvent);
            EventSubscription subscription;
            if (subscriptions.TryGetValue(eventType, out subscription))
            {
                ((EventSubscription<TEvent>) subscription).Unsubscribe(handler);
            }
            else
            {
                logger.Info("No subscribers for event {0} found.".FormatString(eventType.ToString()));
            }
        }

        #endregion

        public void Publish(IPublishableEvent eventMessage)
        {
            Publish<IPublishableEvent>(eventMessage);
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            foreach (var eventMessage in eventMessages)
            {
                Publish(eventMessage);
            }
        }

        public void PublishUncommitedEventsFromAggregateRoot(IAggregateRoot aggregateRoot, string origin)
        {
            var uncommittedChanges = aggregateRoot.GetUncommittedChanges().ToArray();
            Publish(uncommittedChanges);

            aggregateRoot.MarkChangesAsCommitted();
        }

        public void Publish<TEvent>(TEvent @event)
        {
            Publish(typeof(TEvent), @event);
        }

        public void Publish(Type eventType, object @event)
        {
            logger.Info("Event {0} published".FormatString(eventType.ToString()));

            EventSubscription subscription;
            if (subscriptions.TryGetValue(eventType, out subscription))
            {
                subscription.RaiseEvent(@event);
            }
            else
            {
                logger.Info("No subscribers for event {0} found.".FormatString(eventType.ToString()));
            }
        }
    }

    public abstract class EventSubscription
    {
        public abstract void RaiseEvent(object @event);
    }

    public interface IEventSubscription<TEvent> 
    {
        void Subscribe(Action<TEvent> handler);

        void Unsubscribe(Action<TEvent> handler);
    }

    class EventSubscription<TEvent> : EventSubscription, IEventSubscription<TEvent>, IDisposable
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

        public override void RaiseEvent(object @event)
        {
            Action<TEvent>[] actions;
            lock (lockObject)
            {
                EnsureIsNotDisposed();
                actions = eventHandlers.Data;
            }

            if (actions == null || actions.Length == 0)
                return;

            TEvent eventObj = (TEvent) @event;
            foreach (Action<TEvent> action in actions)
            {
                FireEventAsync(action, eventObj);
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