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
using WB.Core.Infrastructure.EventBus.Lite;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    public class InProcessEventBus : IEventBus
    {
        protected struct EventHandlerMethod
        {
            public Type EventType { get; set; }
            public Type EventHandlerType { get; set; }
            public Action<IPublishableEvent> Handle { get; set; }
        }

        private readonly List<EventHandlerMethod> eventHandlerMethods = new List<EventHandlerMethod>();
        //private readonly IEventStore eventStore;
        private readonly EventBusSettings eventBusSettings;
        private readonly ILogger logger;

        public InProcessEventBus(/*IEventStore eventStore,*/ EventBusSettings eventBusSettings, ILogger logger)
        {
            //this.eventStore = eventStore;
            this.eventBusSettings = eventBusSettings;
            this.logger = logger;
        }

        public event EventHandlerExceptionDelegate OnCatchingNonCriticalEventHandlerException;

        public void Publish(IPublishableEvent eventMessage)
        {
            if (eventMessage?.Payload == null) return;

            if(this.eventBusSettings.IgnoredAggregateRoots.Contains(eventMessage.EventSourceId.FormatGuid()))
                return;

            var eventHandlerMethodsByEventType = this.eventHandlerMethods.Where(
                    eventHandlerMethod => eventHandlerMethod.EventType.GetTypeInfo().IsAssignableFrom(eventMessage.Payload.GetType().GetTypeInfo())).ToList();

            if (eventHandlerMethodsByEventType.Any())
            {
                PublishToHandlers(eventMessage, eventHandlerMethodsByEventType);
            }
        }

        public bool CanHandleEvent(IPublishableEvent eventMessage)
        {
            if (eventMessage?.Payload == null) return false;

            if (this.eventBusSettings.IgnoredAggregateRoots.Contains(eventMessage.EventSourceId.FormatGuid()))
                return false;

            return this.eventHandlerMethods.Any(eventHandlerMethod => eventHandlerMethod.EventType.GetTypeInfo().IsAssignableFrom(eventMessage.Payload.GetType().GetTypeInfo()));
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            foreach (var eventMessage in eventMessages)
            {
                this.Publish(eventMessage);
            }
        }

        public IEnumerable<CommittedEvent> CommitUncommittedEvents(IEventSourcedAggregateRoot aggregateRoot, string origin)
        {
            throw new NotImplementedException();

            /*var eventStream = new UncommittedEventStream(origin, aggregateRoot.GetUnCommittedChanges());

            return this.eventStore.Store(eventStream);*/
        }

        public void PublishCommittedEvents(IEnumerable<CommittedEvent> committedEvents) => this.Publish(committedEvents);

        public virtual void RegisterHandler<TEvent>(IEventHandler<TEvent> handler)
            where TEvent: WB.Core.Infrastructure.EventBus.IEvent
        {
            var eventDataType = typeof(TEvent);
            var eventHandlerType = handler.GetType();

            RegisterHandler(eventDataType, eventHandlerType,  evnt => handler.Handle((IPublishedEvent<TEvent>)evnt));
        }

        public void RegisterHandler(Type eventType, Type eventHandlerType, Action<IPublishableEvent> handle)
        {
            this.eventHandlerMethods.Add(new EventHandlerMethod
            {
                EventType = eventType,
                EventHandlerType = eventHandlerType,
                Handle = handle
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
