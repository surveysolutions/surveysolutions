using System;
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

        public EventSourcedAggregateRoot Load(Type aggreateRootType, Snapshot snapshot, CommittedEventStream eventStream)
        {
            EventSourcedAggregateRoot aggregate = null;

            if (!_aggregateSnapshotter.TryLoadFromSnapshot(aggreateRootType, snapshot, eventStream, out aggregate))
                aggregate = GetByIdFromScratch(aggreateRootType, eventStream);

            return aggregate;
        }

        public Snapshot TryTakeSnapshot(IEventSourcedAggregateRoot aggregateRoot)
        {
            Snapshot snapshot = null;
            _aggregateSnapshotter.TryTakeSnapshot(aggregateRoot, out snapshot);
            return snapshot;
        }

        protected EventSourcedAggregateRoot GetByIdFromScratch(Type aggregateRootType, CommittedEventStream committedEventStream)
        {
            EventSourcedAggregateRoot aggregateRoot = null;

            if (committedEventStream.Count() > 0)
            {
                aggregateRoot = (EventSourcedAggregateRoot) this.serviceLocator.GetInstance(aggregateRootType);

                var mappedAggregateRoot = aggregateRoot as MappedAggregateRoot;
                if (mappedAggregateRoot != null
                    && !mappedAggregateRoot.CanApplyHistory(committedEventStream))
                    return null;
                
                aggregateRoot.InitializeFromHistory(committedEventStream);
            }

            return aggregateRoot;
        }

    }
}
