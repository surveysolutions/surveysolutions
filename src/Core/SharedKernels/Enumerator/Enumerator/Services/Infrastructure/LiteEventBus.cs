using System;
using System.Collections.Generic;
using System.Reflection;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Services;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    internal class LiteEventBus : ILiteEventBus
    {
        private readonly IEventStore eventStore;
        private readonly IDenormalizerRegistry denormalizerRegistry;
        private readonly IAsyncEventQueue viewModelEventQueue;
        private readonly ILogger logger;

        public LiteEventBus(IEventStore eventStore, 
            IDenormalizerRegistry denormalizerRegistry,
            IAsyncEventQueue viewModelEventQueue,
            ILogger logger)
        {
            this.eventStore = eventStore;
            this.denormalizerRegistry = denormalizerRegistry;
            this.viewModelEventQueue = viewModelEventQueue;
            this.logger = logger;
        }

        public IReadOnlyCollection<CommittedEvent> CommitUncommittedEvents(IEventSourcedAggregateRoot aggregateRoot, string origin) =>
            this.eventStore.Store(new UncommittedEventStream(origin, aggregateRoot.GetUnCommittedChanges()));

        public void PublishCommittedEvents(IReadOnlyCollection<CommittedEvent> committedEvents)
        {
            this.PublishToDenormalizers(committedEvents);

            this.viewModelEventQueue.Enqueue(committedEvents);
        }

        private void PublishToDenormalizers(IEnumerable<CommittedEvent> committedEvents)
        {
            var exceptions = new List<Exception>();
            foreach (var uncommittedChange in committedEvents)
            {
                var denormalizers = this.denormalizerRegistry.GetDenormalizers(uncommittedChange);
                foreach (var denormalizer in denormalizers)
                {
                    try
                    {
                        var eventType = uncommittedChange.Payload.GetType();

                        var publishedEventType = typeof(PublishedEvent<>).MakeGenericType(eventType);
                        var publishedEvent = Activator.CreateInstance(publishedEventType, uncommittedChange);

                        var publishedEventInterfaceType = typeof(IPublishedEvent<>).MakeGenericType(eventType);
                        var method = denormalizer.GetType().GetRuntimeMethod("Handle", new[] { publishedEventInterfaceType });

                        this.logger.Debug($"Publishing {uncommittedChange.Payload.GetType().Name} to {denormalizer.GetType().Name}");
                        method.Invoke(denormalizer, new[] { publishedEvent });
                    }
                    catch (Exception exception)
                    {
                        exceptions.Add(exception);
                    }
                }

                if (denormalizers.Count == 0)
                {
                    this.logger.Debug($"No denormalizers registered for {uncommittedChange.Payload.GetType().Name} event");
                } 

                if (exceptions.Count > 0)
                {
                    var message = string.Format("{0} handler(s) failed to handle published event '{1}' of type '{4} by event source '{2}' with sequence '{3}'.",
                        exceptions.Count, uncommittedChange.EventIdentifier, uncommittedChange.EventSourceId, uncommittedChange.EventSequence, uncommittedChange.Payload.GetType().Name);
                    throw new AggregateException(message, exceptions);
                }
            }
        }
    }
}
