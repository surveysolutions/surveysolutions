using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    public class InProcessEventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Action<PublishedEvent>>> _handlerRegister = new Dictionary<Type, List<Action<PublishedEvent>>>();
        private readonly bool _useTransactionScope;
        private readonly IEventStore eventStore;

        public InProcessEventBus(IEventStore eventStore)
            : this(true, eventStore)
        {            
        }

        /// <summary>
        /// Creates new <see cref="InProcessEventBus"/> instance.
        /// </summary>
        /// <param name="useTransactionScope">Use transaction scope?</param>
        /// <param name="eventStore">Event store</param>
        public InProcessEventBus(bool useTransactionScope, IEventStore eventStore)
        {
            _useTransactionScope = useTransactionScope;
            this.eventStore = eventStore;
        }

        public void Publish(IPublishableEvent eventMessage)
        {
            var eventMessageType = eventMessage.GetType();

            List<Action<PublishedEvent>> handlers = GetHandlersForEvent(eventMessage);
            
            if (handlers.Any())
            {
                if (_useTransactionScope)
                {
                    TransactionallyPublishToHandlers(eventMessage, eventMessageType, handlers);
                }
                else
                {
                    PublishToHandlers(eventMessage, eventMessageType, handlers);
                }
            }
        }

        private static void TransactionallyPublishToHandlers(IPublishableEvent eventMessage, Type eventMessageType, IEnumerable<Action<PublishedEvent>> handlers)
        {
            PublishToHandlers(eventMessage, eventMessageType, handlers);
        }

        private static void PublishToHandlers(IPublishableEvent eventMessage, Type eventMessageType, IEnumerable<Action<PublishedEvent>> handlers)
        {
            var publishedEventClosedType = typeof (PublishedEvent<>).MakeGenericType(eventMessage.Payload.GetType());
            var publishedEvent = (PublishedEvent)Activator.CreateInstance(publishedEventClosedType, eventMessage);

            var occurredExceptions = new List<Exception>();

            foreach (var handler in handlers)
            {
                try
                {
                    handler.Invoke(publishedEvent);
                }
                catch (Exception exception)
                {
                    occurredExceptions.Add(exception);
                }
            }

            if (occurredExceptions.Count > 0)
                throw new AggregateException(
                    string.Format("{0} handler(s) failed to handle published event '{1}'.", occurredExceptions.Count, eventMessage.EventIdentifier),
                    occurredExceptions);
        }

        [ContractVerification(false)]
        protected List<Action<PublishedEvent>> GetHandlersForEvent(IPublishableEvent eventMessage)
        {
            if (eventMessage == null)
                return null;

            var dataType = eventMessage.Payload.GetType();
            var result = new List<Action<PublishedEvent>>();

            foreach(var key in _handlerRegister.Keys)
            {
                if (key.GetTypeInfo().IsAssignableFrom(dataType.GetTypeInfo()))
                {
                    var handlers = _handlerRegister[key];
                    result.AddRange(handlers);
                }
            }

            return result;
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
            var eventStream = new UncommittedEventStream(origin);

            foreach (UncommittedEvent @event in aggregateRoot.GetUncommittedChanges())
            {
                eventStream.Append(@event);
            }

            this.eventStore.Store(eventStream);
            this.Publish(eventStream);

            aggregateRoot.MarkChangesAsCommitted();
        }

        public virtual void RegisterHandler<TEvent>(IEventHandler<TEvent> handler)
        {
            var eventDataType = typeof(TEvent);

            RegisterHandler(eventDataType, evnt => handler.Handle((IPublishedEvent<TEvent>)evnt));
        }

        public void RegisterHandler(Type eventDataType, Action<PublishedEvent> handler)
        {
            List<Action<PublishedEvent>> handlers = null;
            if (!_handlerRegister.TryGetValue(eventDataType, out handlers))
            {
                handlers = new List<Action<PublishedEvent>>(1);
                _handlerRegister.Add(eventDataType, handlers);
            }

            handlers.Add(handler);
        }
    }
}