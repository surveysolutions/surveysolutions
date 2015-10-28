using System;
using System.Collections.Concurrent;
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
        private readonly Dictionary<Type, List<Action<IPublishableEvent>>> handlerRegistry = new Dictionary<Type, List<Action<IPublishableEvent>>>();
        private readonly IEventStore eventStore;

        public InProcessEventBus(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public void Publish(IPublishableEvent eventMessage)
        {
            List<Action<IPublishableEvent>> handlers = GetHandlersForEvent(eventMessage);

            if (handlers.Any())
            {
                PublishToHandlers(eventMessage, handlers);
            }
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            foreach (var eventMessage in eventMessages)
            {
                this.Publish(eventMessage);
            }
        }

        public CommittedEventStream CommitUncommittedEvents(IAggregateRoot aggregateRoot, string origin)
        {
            var eventStream = new UncommittedEventStream(origin, aggregateRoot.GetUnCommittedChanges());

            return this.eventStore.Store(eventStream);
        }

        public void PublishCommittedEvents(CommittedEventStream committedEvents)
        {
            this.Publish(committedEvents);
        }

        public virtual void RegisterHandler<TEvent>(IEventHandler<TEvent> handler)
        {
            var eventDataType = typeof(TEvent);

            RegisterHandler(eventDataType, evnt => handler.Handle((IPublishedEvent<TEvent>)evnt));
        }

        public void RegisterHandler(Type eventDataType, Action<IPublishableEvent> handler)
        {
            List<Action<IPublishableEvent>> handlers = null;
            if (!this.handlerRegistry.TryGetValue(eventDataType, out handlers))
            {
                handlers = new List<Action<IPublishableEvent>>(1);
                this.handlerRegistry.Add(eventDataType, handlers);
            }

            handlers.Add(handler);
        }

        [ContractVerification(false)]
        protected List<Action<IPublishableEvent>> GetHandlersForEvent(IPublishableEvent eventMessage)
        {
            if (eventMessage == null)
                return null;

            var dataType = eventMessage.Payload.GetType();
            var result = new List<Action<IPublishableEvent>>();

            foreach (var key in this.handlerRegistry.Keys)
            {
                if (key.GetTypeInfo().IsAssignableFrom(dataType.GetTypeInfo()))
                {
                    List<Action<IPublishableEvent>> handlers = this.handlerRegistry[key];
                    result.AddRange(handlers);
                }
            }

            return result;
        }

        private static void PublishToHandlers(IPublishableEvent eventMessage, IEnumerable<Action<IPublishableEvent>> handlers)
        {
            var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(eventMessage.Payload.GetType());
            IPublishableEvent publishedEvent = (IPublishableEvent)Activator.CreateInstance(publishedEventClosedType, eventMessage);

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
                   string.Format("{0} handler(s) failed to handle published event '{1}' by event source '{2}' with sequence '{3}'.", occurredExceptions.Count, eventMessage.EventIdentifier, eventMessage.EventSourceId, eventMessage.EventSequence),
                    occurredExceptions);
        }
    }
}