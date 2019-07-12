using System;
using System.Collections.Generic;
using System.Reflection;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    internal class LiteEventBus : ILiteEventBus
    {
        private readonly IEventStore eventStore;
        private readonly IDenormalizerRegistry denormalizerRegistry;
        private readonly ViewModelEventQueue viewModelEventQueue;

        public LiteEventBus(IEventStore eventStore, 
            IDenormalizerRegistry denormalizerRegistry,
            ViewModelEventQueue viewModelEventQueue)
        {
            this.eventStore = eventStore;
            this.denormalizerRegistry = denormalizerRegistry;
            this.viewModelEventQueue = viewModelEventQueue;
        }

        public IEnumerable<CommittedEvent> CommitUncommittedEvents(IEventSourcedAggregateRoot aggregateRoot, string origin) =>
            this.eventStore.Store(new UncommittedEventStream(origin, aggregateRoot.GetUnCommittedChanges()));

        public void PublishCommittedEvents(IEnumerable<CommittedEvent> committedEvents)
        {
            this.PublishToDenormalizers(committedEvents);

            foreach (var committedEvent in committedEvents)
                this.viewModelEventQueue.Enqueue(committedEvent);
        }

        private void PublishToDenormalizers(IEnumerable<CommittedEvent> committedEvents)
        {
            var exceptions = new List<Exception>();
            foreach (var uncommittedChange in committedEvents)
            {
                foreach (var denormalizer in this.denormalizerRegistry.GetDenormalizers(uncommittedChange))
                {
                    try
                    {
                        var eventType = uncommittedChange.Payload.GetType();

                        var publishedEventType = typeof(PublishedEvent<>).MakeGenericType(eventType);
                        var publishedEvent = Activator.CreateInstance(publishedEventType, uncommittedChange);

                        var publishedEventInterfaceType = typeof(IPublishedEvent<>).MakeGenericType(eventType);
                        var method = denormalizer.GetType().GetRuntimeMethod("Handle", new[] { publishedEventInterfaceType });

                        method.Invoke(denormalizer, new[] { publishedEvent });
                    }
                    catch (Exception exception)
                    {
                        exceptions.Add(exception);
                    }
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
