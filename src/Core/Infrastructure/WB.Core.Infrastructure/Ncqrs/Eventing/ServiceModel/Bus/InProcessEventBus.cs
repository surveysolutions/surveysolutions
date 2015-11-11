using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Ncqrs.Domain;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    public class InProcessEventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Action<IPublishableEvent>>> handlerRegistry = new Dictionary<Type, List<Action<IPublishableEvent>>>();
        private readonly IEventStore eventStore;
        private readonly EventBusSettings eventBusSettings;
        private readonly ILogger logger;

        public InProcessEventBus(IEventStore eventStore, EventBusSettings eventBusSettings, ILogger logger)
        {
            this.eventStore = eventStore;
            this.eventBusSettings = eventBusSettings;
            this.logger = logger;
        }

        public void Publish(IPublishableEvent eventMessage, Action<EventHandlerException> onCatchingNonCriticalEventHandlerException = null)
        {
            List<Action<IPublishableEvent>> handlers = GetHandlersForEvent(eventMessage);

            if (handlers.Any())
            {
                PublishToHandlers(eventMessage, handlers, onCatchingNonCriticalEventHandlerException);
            }
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            foreach (var eventMessage in eventMessages)
            {
                this.Publish(eventMessage, null);
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

            this.handlerByEventType.Add(eventDataType, handler as IEventHandler);

            RegisterHandler(eventDataType, evnt => handler.Handle((IPublishedEvent<TEvent>)evnt));
        }

        private readonly Dictionary<Type, IEventHandler> handlerByEventType = new Dictionary<Type, IEventHandler>();    

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

        private void PublishToHandlers(IPublishableEvent eventMessage, IEnumerable<Action<IPublishableEvent>> handlers, Action<EventHandlerException> onCatchingNonCriticalEventHandlerException)
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
                    var eventHandler = this.handlerByEventType[eventMessage.Payload.GetType()];

                    var catchEventHandlerException = eventHandler != null && this.eventBusSettings.CatchExceptionsByEventHandlerTypes.Contains(eventHandler.GetType());

                    var eventHandlerException = new EventHandlerException(eventHandlerType: eventHandler?.GetType(),
                        eventType: eventMessage.Payload.GetType(), isCritical: !catchEventHandlerException,
                        innerException: exception);

                    if (catchEventHandlerException)
                    {
                        this.logger.Error($"Failed to handle {eventHandlerException.EventType.Name} in {eventHandlerException.EventHandlerType} for event '{eventMessage.EventIdentifier}' by event source '{eventMessage.EventSourceId}' with sequence '{eventMessage.EventSequence}'.", eventHandlerException);
                        onCatchingNonCriticalEventHandlerException?.Invoke(eventHandlerException);
                    }
                    else
                    {
                        occurredExceptions.Add(eventHandlerException);
                    }
                }
            }

            if (occurredExceptions.Count > 0)
            {
                throw new AggregateException($"{occurredExceptions.Count} handler(s) failed to handle published event '{eventMessage.EventIdentifier}' by event source '{eventMessage.EventSourceId}' with sequence '{eventMessage.EventSequence}'.", occurredExceptions);
            }
        }
    }
}