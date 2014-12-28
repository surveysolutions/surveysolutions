using System;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Domain.Storage
{
    public class DomainRepository : IDomainRepository
    {
        private readonly IAggregateRootCreationStrategy _aggregateRootCreator;

        private readonly IAggregateSnapshotter _aggregateSnapshotter;

        public DomainRepository(IAggregateRootCreationStrategy aggregateRootCreationStrategy, IAggregateSnapshotter aggregateSnapshotter)
        {
            _aggregateRootCreator = aggregateRootCreationStrategy;
            _aggregateSnapshotter = aggregateSnapshotter;
        }

        public AggregateRoot Load(Type aggreateRootType, Snapshot snapshot, CommittedEventStream eventStream)
        {
            AggregateRoot aggregate = null;

            if (!_aggregateSnapshotter.TryLoadFromSnapshot(aggreateRootType, snapshot, eventStream, out aggregate))
                aggregate = GetByIdFromScratch(aggreateRootType, eventStream);

            return aggregate;
        }

        public Snapshot TryTakeSnapshot(IAggregateRoot aggregateRoot)
        {
            Snapshot snapshot = null;
            _aggregateSnapshotter.TryTakeSnapshot(aggregateRoot, out snapshot);
            return snapshot;
        }

        protected AggregateRoot GetByIdFromScratch(Type aggregateRootType, CommittedEventStream committedEventStream)
        {
            AggregateRoot aggregateRoot = null;

            if (committedEventStream.Count() > 0)
            {
                aggregateRoot = _aggregateRootCreator.CreateAggregateRoot(aggregateRootType);
                aggregateRoot.InitializeFromHistory(committedEventStream);
            }

            return aggregateRoot;
        }

    }
}
