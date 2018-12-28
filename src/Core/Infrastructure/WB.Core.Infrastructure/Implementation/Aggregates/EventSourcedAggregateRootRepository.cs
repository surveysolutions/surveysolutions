using System;
using System.Collections.Generic;
using System.Threading;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    public class EventSourcedAggregateRootRepository : IEventSourcedAggregateRootRepository
    {
        private readonly IEventStore eventStore;
        protected readonly IDomainRepository repository;

        public EventSourcedAggregateRootRepository(IEventStore eventStore, IDomainRepository repository)
        {
            this.eventStore = eventStore;
            this.repository = repository;
        }

        public virtual IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
            => this.GetLatest(aggregateType, aggregateId, null, CancellationToken.None);

        public virtual IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            IEnumerable<CommittedEvent> events = this.eventStore.Read(aggregateId, 0, progress, cancellationToken);

            return this.repository.Load(aggregateType, aggregateId, events);
        }

        public virtual IEventSourcedAggregateRoot GetStateless(Type aggregateType, Guid aggregateId)
        {
            int? lastEventSequence = this.eventStore.GetLastEventSequence(aggregateId);
            if (!lastEventSequence.HasValue)
                return null;
            return this.repository.LoadStateless(aggregateType, aggregateId, lastEventSequence.Value);
        }
    }
}
