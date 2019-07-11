using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public class LiteEventBus : ILiteEventBus
    {
        private readonly IViewModelEventRegistry liteEventRegistry;
        private readonly IEventStore eventStore;
        private readonly IDenormalizerRegistry denormalizerRegistry;

        public LiteEventBus(IViewModelEventRegistry liteEventRegistry, IEventStore eventStore, IDenormalizerRegistry denormalizerRegistry)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.eventStore = eventStore;
            this.denormalizerRegistry = denormalizerRegistry;
        }

        public IEnumerable<CommittedEvent> CommitUncommittedEvents(IEventSourcedAggregateRoot aggregateRoot, string origin) =>
            this.eventStore.Store(new UncommittedEventStream(origin, aggregateRoot.GetUnCommittedChanges()));

        public void PublishCommittedEvents(IEnumerable<CommittedEvent> committedEvents)
        {
            this.PublishToDenormalizers(committedEvents);
            this.PublishToViewModels(committedEvents);
        }

        private void PublishToViewModels(IEnumerable<CommittedEvent> committedEvents) =>
            Task.Run(async () => await this.PublishToViewModelsAsync(committedEvents));

        private async Task PublishToViewModelsAsync(IEnumerable<CommittedEvent> committedEvents)
        {
            var exceptions = new List<Exception>();
            foreach (var uncommittedChange in committedEvents)
            foreach (var viewModel in this.liteEventRegistry.GetViewModelsByEvent(uncommittedChange))
            {
                try
                {
                    var handler = viewModel.GetType().GetRuntimeMethod("Handle", new[] {uncommittedChange.Payload.GetType()});
                    
                    var taskOrVoid = (Task)handler?.Invoke(viewModel, new object[] {uncommittedChange.Payload});
                    if(taskOrVoid != null) await taskOrVoid;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException("Exception during update view models", exceptions);
            }
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
