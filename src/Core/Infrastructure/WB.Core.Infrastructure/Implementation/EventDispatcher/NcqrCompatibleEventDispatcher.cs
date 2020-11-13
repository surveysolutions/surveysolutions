using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.Services;

namespace WB.Core.Infrastructure.Implementation.EventDispatcher
{
    public class NcqrCompatibleEventDispatcher : IEventBus
    {
        private readonly Type[] handlersToIgnore;

        private readonly EventBusSettings eventBusSettings;
        private readonly ILogger logger;
        private readonly IServiceLocator serviceLocator;
        private readonly IEventStore eventStore;
        private readonly IInMemoryEventStore inMemoryEventStore;
        private readonly IDenormalizerRegistry denormalizerRegistry;
        private readonly IAggregateRootPrototypeService prototypeService;

        public NcqrCompatibleEventDispatcher(
            IServiceLocator serviceLocator,
            EventBusSettings eventBusSettings,
            ILogger logger,
            IEventStore eventStore,
            IInMemoryEventStore inMemoryEventStore,
            IDenormalizerRegistry denormalizerRegistry,
            IAggregateRootPrototypeService prototypeService)
        {
            this.eventBusSettings = eventBusSettings;
            this.logger = logger;
            this.handlersToIgnore = eventBusSettings.DisabledEventHandlerTypes;
            this.eventStore = eventStore;
            this.inMemoryEventStore = inMemoryEventStore;
            this.denormalizerRegistry = denormalizerRegistry;
            this.prototypeService = prototypeService;
            this.serviceLocator = serviceLocator;
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            List<IPublishableEvent> events = eventMessages.ToList();

            if (!events.Any())
                return;

            Guid firstEventSourceId = events.First().EventSourceId;

            var errorsDuringHandling = new List<Exception>();

            if (this.prototypeService.GetPrototypeType(firstEventSourceId) != PrototypeType.Temporary)
            {
                foreach (var functionalEventHandler in denormalizerRegistry.FunctionalDenormalizers)
                {
                    var handler = (IFunctionalEventHandler)this.serviceLocator.GetInstance(functionalEventHandler);

                    try
                    {
                        handler.Handle(events);
                    }
                    catch (Exception exception)
                    {
                        var eventHandlerType = handler.GetType();
                        var shouldIgnoreException =
                            this.eventBusSettings.EventHandlerTypesWithIgnoredExceptions.Contains(eventHandlerType);

                        var eventHandlerException = new EventHandlerException(eventHandlerType: eventHandlerType,
                            eventType: events.First().GetType(), isCritical: !shouldIgnoreException,
                            innerException: exception);

                        if (shouldIgnoreException)
                        {
                            this.logger.Error(
                                $"Failed to handle {eventHandlerException.EventType.Name} in " +
                                $"{eventHandlerException.EventHandlerType} by event source '{firstEventSourceId}'.",
                                eventHandlerException);

                        }
                        else
                        {
                            errorsDuringHandling.Add(eventHandlerException);
                        }
                    }
                }
            }

            foreach (IPublishableEvent publishableEvent in events)
            {
                foreach (Type handler in denormalizerRegistry.SequentialDenormalizers.Where(x => !handlersToIgnore.Contains(x)))
                {
                    if (!denormalizerRegistry.CanHandleEvent(handler, publishableEvent))
                        continue;

                    try
                    {
                        if (publishableEvent?.Payload == null)
                        {
                            continue;
                        }

                        bool isPrototype = this.prototypeService.IsPrototype(publishableEvent.EventSourceId);

                        var eventType = publishableEvent.Payload.GetType();
                        var eventHandlerMethod = denormalizerRegistry.HandlerMethod(handler, eventType);

                        if (isPrototype && !eventHandlerMethod.ReceivesIgnoredEvents)
                        {
                            continue;
                        }

                        var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(eventType);
                        var publishedEvent = Activator.CreateInstance(publishedEventClosedType, publishableEvent);

                        List<Exception> occurredExceptions = null;
                        try
                        {
                            var denormalizerInstance = this.serviceLocator.GetInstance(handler);
                            eventHandlerMethod.Handle.Invoke(denormalizerInstance, new[] {publishedEvent});
                        }
                        catch (Exception exception)
                        {
                            var shouldIgnoreException =
                                this.eventBusSettings.EventHandlerTypesWithIgnoredExceptions.Contains(handler);

                            var eventHandlerException = new EventHandlerException(eventHandlerType: handler,
                                eventType: eventType,
                                isCritical: !shouldIgnoreException,
                                innerException: exception);

                            if (shouldIgnoreException)
                            {
                                this.logger.Error(
                                    $"Failed to handle {eventHandlerException.EventType.Name} in {eventHandlerException.EventHandlerType} for event '{publishableEvent.EventIdentifier}' by event source '{publishableEvent.EventSourceId}' with sequence '{publishableEvent.EventSequence}'.",
                                    eventHandlerException);
                            }
                            else
                            {
                                if (occurredExceptions == null)
                                {
                                    occurredExceptions = new List<Exception>();
                                }

                                occurredExceptions.Add(eventHandlerException);
                            }
                        }

                        if (occurredExceptions?.Count > 0)
                        {
                            throw new AggregateException(
                                $"{occurredExceptions.Count} handler(s) failed to handle published event '{publishableEvent.EventIdentifier}' by event source '{publishableEvent.EventSourceId}' with sequence '{publishableEvent.EventSequence}'.",
                                occurredExceptions);
                        }
                    }
                    catch (Exception exception)
                    {
                        errorsDuringHandling.Add(exception);
                    }
                }
            }

            if (errorsDuringHandling.Count > 0)
                throw new AggregateException(
                    $"One or more handlers failed when publishing {events.Count} events. First event source id: {firstEventSourceId.FormatGuid()}.",
                    errorsDuringHandling);
        }

        public IReadOnlyCollection<CommittedEvent> CommitUncommittedEvents(IEventSourcedAggregateRoot aggregateRoot, string origin)
        {
            var eventStream = new UncommittedEventStream(origin, aggregateRoot.GetUnCommittedChanges());
            if (this.prototypeService.IsPrototype(aggregateRoot.EventSourceId))
            {
                return this.inMemoryEventStore.Store(eventStream);
            }

            return this.eventStore.Store(eventStream);
        }

        public void PublishCommittedEvents(IReadOnlyCollection<CommittedEvent> committedEvents) => this.Publish(committedEvents);
    }
}
