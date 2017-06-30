using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Mapping;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Domain.Storage
{
    public class DomainRepository : IDomainRepository
    {
        private readonly IAggregateSnapshotter _aggregateSnapshotter;
        private readonly IServiceLocator serviceLocator;

        public DomainRepository(IAggregateSnapshotter aggregateSnapshotter, IServiceLocator serviceLocator)
        {
            _aggregateSnapshotter = aggregateSnapshotter;
            this.serviceLocator = serviceLocator;
        }

        public IEventSourcedAggregateRoot Load(Type aggreateRootType, Snapshot snapshot, CommittedEventStream eventStream)
            => this.Load(aggreateRootType, eventStream.SourceId, snapshot, eventStream);

        public IEventSourcedAggregateRoot Load(Type aggreateRootType, Guid aggregateRootId, Snapshot snapshot, IEnumerable<CommittedEvent> events)
        {
            EventSourcedAggregateRoot aggregate;

            if (!_aggregateSnapshotter.TryLoadFromSnapshot(aggreateRootType, snapshot, events, out aggregate))
            {
                aggregate = this.GetByIdFromScratch(aggreateRootType, aggregateRootId, events);
            }

            return aggregate;
        }

        public IEventSourcedAggregateRoot LoadStateless(Type aggregateRootType, Guid aggregateRootId, int lastEventSequence)
        {
            var aggregateRoot = (EventSourcedAggregateRoot)this.serviceLocator.GetInstance(aggregateRootType);

            if (aggregateRoot == null)
                throw new ArgumentException($"Cannot create new instance of aggregate root of type {aggregateRootType.Name}");

            aggregateRoot.InitializeFromSnapshot(new Snapshot(aggregateRootId, lastEventSequence, null));

            return aggregateRoot;
        }

        public Snapshot TryTakeSnapshot(IEventSourcedAggregateRoot aggregateRoot)
        {
            Snapshot snapshot = null;
            _aggregateSnapshotter.TryTakeSnapshot(aggregateRoot, out snapshot);
            return snapshot;
        }

        private EventSourcedAggregateRoot GetByIdFromScratch(Type aggregateRootType, Guid aggregateRootId, IEnumerable<CommittedEvent> events)
        {
            var aggregateRoot = (EventSourcedAggregateRoot) this.serviceLocator.GetInstance(aggregateRootType);

            if (aggregateRoot == null)
                throw new ArgumentException($"Cannot create new instance of aggregate root of type {aggregateRootType.Name}");

            aggregateRoot.InitializeFromHistory(aggregateRootId, events);

            bool atLeastOneEventApplied = aggregateRoot.InitialVersion > 0;

            return atLeastOneEventApplied ? aggregateRoot : null;
        }

    }
}
