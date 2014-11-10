using System;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.EventBus
{
    // TODO: TLK, KP-4337: include it's functionality to event bus when it will be made portable
    public class EventPublisher : IEventPublisher
    {
        private readonly IEventStore eventStore;
        private readonly IEventBus eventBus;

        public EventPublisher(IEventStore eventStore, IEventBus eventBus)
        {
            this.eventStore = eventStore;
            this.eventBus = eventBus;
        }

        public void PublishUncommitedEventsFromAggregateRoot(IAggregateRoot aggregateRoot, string origin)
        {
            var eventStream = new UncommittedEventStream(origin);
            var aggregate = (AggregateRoot)aggregateRoot; // TODO: TLK, KP-4337: move GetUncommittedChanges method to interface when UncommittedEvent will be portable

            foreach (UncommittedEvent @event in aggregate.GetUncommittedChanges())
            {
                eventStream.Append(@event);
            }

            this.eventStore.Store(eventStream);
            this.eventBus.Publish(eventStream);

            aggregateRoot.MarkChangesAsCommitted();
        }
    }
}