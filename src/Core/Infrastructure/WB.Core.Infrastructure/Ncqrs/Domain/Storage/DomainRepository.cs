using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Mapping;
using Ncqrs.Eventing.Sourcing.Snapshotting;
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

        public AggregateRoot Load(Type aggreateRootType, Snapshot snapshot, CommittedEventStream eventStream)
            => this.Load(aggreateRootType, eventStream.SourceId, snapshot, eventStream);

        public AggregateRoot Load(Type aggreateRootType, Guid aggregateRootId, Snapshot snapshot, IEnumerable<CommittedEvent> events)
        {
            AggregateRoot aggregate;

            if (!_aggregateSnapshotter.TryLoadFromSnapshot(aggreateRootType, snapshot, events, out aggregate))
            {
                aggregate = this.GetByIdFromScratch(aggreateRootType, aggregateRootId, events);
            }

            return aggregate;
        }

        public Snapshot TryTakeSnapshot(IAggregateRoot aggregateRoot)
        {
            Snapshot snapshot = null;
            _aggregateSnapshotter.TryTakeSnapshot(aggregateRoot, out snapshot);
            return snapshot;
        }

        private AggregateRoot GetByIdFromScratch(Type aggregateRootType, Guid aggregateRootId, IEnumerable<CommittedEvent> events)
        {
            var aggregateRoot = (AggregateRoot) this.serviceLocator.GetInstance(aggregateRootType);

            if (aggregateRoot == null)
                throw new ArgumentException($"Cannot create new instance of aggregate root of type {aggregateRootType.Name}");

            aggregateRoot.InitializeFromHistory(aggregateRootId, events);

            bool atLeastOneEventApplied = aggregateRoot.InitialVersion > 0;

            return atLeastOneEventApplied ? aggregateRoot : null;
        }

    }
}
