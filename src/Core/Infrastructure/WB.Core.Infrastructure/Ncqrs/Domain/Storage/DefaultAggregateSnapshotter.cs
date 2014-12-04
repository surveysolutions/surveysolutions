using System;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Domain.Storage
{
    public class DefaultAggregateSnapshotter : IAggregateSnapshotter
    {
        private readonly IAggregateRootCreationStrategy _aggregateRootCreator;
        private readonly IAggregateSupportsSnapshotValidator _snapshotValidator;

        private readonly ISnapshottingPolicy snapshottingPolicy;
        private readonly IDomainRepository repository;
        private readonly ISnapshotStore snapshotStore;

        public DefaultAggregateSnapshotter(IAggregateRootCreationStrategy aggregateRootCreationStrategy, IAggregateSupportsSnapshotValidator snapshotValidator,
            ISnapshottingPolicy snapshottingPolicy, IDomainRepository repository, ISnapshotStore snapshotStore)
        {
            _aggregateRootCreator = aggregateRootCreationStrategy;
            _snapshotValidator = snapshotValidator;
            this.snapshottingPolicy = snapshottingPolicy;
            this.repository = repository;
            this.snapshotStore = snapshotStore;
        }

        public bool TryLoadFromSnapshot(Type aggregateRootType, Snapshot snapshot, CommittedEventStream committedEventStream, out AggregateRoot aggregateRoot)
        {
            aggregateRoot = null;

            if (snapshot == null) return false;

            if (AggregateSupportsSnapshot(aggregateRootType, snapshot.Payload.GetType()))
            {
                aggregateRoot = _aggregateRootCreator.CreateAggregateRoot(aggregateRootType);
                aggregateRoot.InitializeFromSnapshot(snapshot);

                var memType = aggregateRoot.GetType().GetSnapshotInterfaceType();
                var restoreMethod = memType.GetMethod("RestoreFromSnapshot");

                restoreMethod.Invoke(aggregateRoot, new[] { snapshot.Payload });

                aggregateRoot.InitializeFromHistory(committedEventStream);

                return true;
            }

            return false;
        }

        public bool TryTakeSnapshot(IAggregateRoot aggregateRoot, out Snapshot snapshot)
        {
            snapshot = null;
            var memType = aggregateRoot.GetType().GetSnapshotInterfaceType();
            if (memType != null)
            {
                var createMethod = memType.GetMethod("CreateSnapshot");
                var payload = createMethod.Invoke(aggregateRoot, new object[0]);
                snapshot = new Snapshot(aggregateRoot.EventSourceId, aggregateRoot.Version, payload);
                return true;
            }
            return false;
        }

        public void CreateSnapshotIfNeededAndPossible(IAggregateRoot aggregateRoot)
        {
            if (!this.snapshottingPolicy.ShouldCreateSnapshot(aggregateRoot))
                return;

            var snapshot = this.repository.TryTakeSnapshot(aggregateRoot);

            if (snapshot == null)
                return;

            this.snapshotStore.SaveShapshot(snapshot);
        }

        private bool AggregateSupportsSnapshot(Type aggregateRootType, Type snapshotType)
        {
            return _snapshotValidator.DoesAggregateSupportsSnapshot(aggregateRootType, snapshotType);
        }

    }
    
}
