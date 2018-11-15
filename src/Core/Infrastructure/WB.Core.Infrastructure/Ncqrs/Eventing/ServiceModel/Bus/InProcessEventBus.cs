using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Domain;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Ncqrs.Eventing;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    public class InProcessEventBus : IEventBus
    {
        protected struct EventHandlerMethod
        {
            public Type EventType { get; set; }
            public Type EventHandlerType { get; set; }

            public bool ReceivesIgnoredEvents { get; set; }

            public Action<IPublishableEvent> Handle { get; set; }
        }

        private readonly List<EventHandlerMethod> eventHandlerMethods = new List<EventHandlerMethod>();
        private readonly IEventStore eventStore;
        private readonly EventBusSettings eventBusSettings;
        private readonly ILogger logger;

        public InProcessEventBus(IEventStore eventStore, EventBusSettings eventBusSettings, ILogger logger)
        {
            this.eventStore = eventStore;
            this.eventBusSettings = eventBusSettings;
            this.logger = logger;
        }

        public event EventHandlerExceptionDelegate OnCatchingNonCriticalEventHandlerException;

        public void Publish(IPublishableEvent eventMessage)
        {
            if (eventMessage?.Payload == null) return;

            bool isIgnoredAggregate =
                this.eventBusSettings.IsIgnoredAggregate(eventMessage.EventSourceId);

            var eventHandlerMethodsByEventType = this.eventHandlerMethods.Where(
                    eventHandlerMethod => eventHandlerMethod.EventType.GetTypeInfo().IsAssignableFrom(eventMessage.Payload.GetType().GetTypeInfo()) && 
                                          (!isIgnoredAggregate || eventHandlerMethod.ReceivesIgnoredEvents))
                    
                            .ToList();

            if (eventHandlerMethodsByEventType.Any())
            {
                PublishToHandlers(eventMessage, eventHandlerMethodsByEventType);
            }
        }

        public IEnumerable<CommittedEvent> CommitUncommittedEvents(IEventSourcedAggregateRoot aggregateRoot, string origin)
        {
            var eventStream = new UncommittedEventStream(origin, aggregateRoot.GetUnCommittedChanges());

            return this.eventStore.Store(eventStream);
        }

        public bool CanHandleEvent(IPublishableEvent eventMessage)
        {
            if (eventMessage?.Payload == null) return false;

            if (this.eventBusSettings.IsIgnoredAggregate(eventMessage.EventSourceId))
            {
                return this.eventHandlerMethods.Any(x => x.ReceivesIgnoredEvents);
            }

            return this.eventHandlerMethods.Any(eventHandlerMethod => eventHandlerMethod.EventType.GetTypeInfo().IsAssignableFrom(eventMessage.Payload.GetType().GetTypeInfo()));
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            foreach (var eventMessage in eventMessages)
            {
                this.Publish(eventMessage);
            }
        }

        public void PublishCommittedEvents(IEnumerable<CommittedEvent> committedEvents) => this.Publish(committedEvents);

        public virtual void RegisterHandler<TEvent>(IEventHandler<TEvent> handler)
            where TEvent: WB.Core.Infrastructure.EventBus.IEvent
        {
            var eventDataType = typeof(TEvent);
            var eventHandlerType = handler.GetType();

            RegisterHandler(eventDataType, eventHandlerType, @event => handler.Handle((IPublishedEvent<TEvent>)@event));
        }

        public void RegisterHandler(Type eventType, Type eventHandlerType, Action<IPublishableEvent> handle)
        {
            var receivesIgnoredEventsAttribute = eventHandlerType.GetCustomAttribute(typeof(ReceivesIgnoredEventsAttribute));
            this.eventHandlerMethods.Add(new EventHandlerMethod
            {
                EventType = eventType,
                EventHandlerType = eventHandlerType,
                Handle = handle,
                ReceivesIgnoredEvents = receivesIgnoredEventsAttribute != null
            });
        }

        private void PublishToHandlers(IPublishableEvent eventMessage, IEnumerable<EventHandlerMethod> eventHandlerMethodsToPublish)
        {
            var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(eventMessage.Payload.GetType());
            IPublishableEvent publishedEvent = (IPublishableEvent)Activator.CreateInstance(publishedEventClosedType, eventMessage);

            var occurredExceptions = new List<Exception>();

            foreach (var eventHandlerMethod in eventHandlerMethodsToPublish)
            {
                try
                {
                    eventHandlerMethod.Handle.Invoke(publishedEvent);
                }
                catch (Exception exception)
                {
                    var shouldIgnoreException = this.eventBusSettings.EventHandlerTypesWithIgnoredExceptions.Contains(eventHandlerMethod.EventHandlerType);

                    var eventHandlerException = new EventHandlerException(eventHandlerType: eventHandlerMethod.EventHandlerType,
                        eventType: eventHandlerMethod.EventType, isCritical: !shouldIgnoreException,
                        innerException: exception);

                    if (shouldIgnoreException)
                    {
                        this.logger.Error($"Failed to handle {eventHandlerException.EventType.Name} in {eventHandlerException.EventHandlerType} for event '{eventMessage.EventIdentifier}' by event source '{eventMessage.EventSourceId}' with sequence '{eventMessage.EventSequence}'.", eventHandlerException);
                        this.OnCatchingNonCriticalEventHandlerException?.Invoke(eventHandlerException);
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
