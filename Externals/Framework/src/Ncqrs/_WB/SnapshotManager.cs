using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Snapshots
{
    // TODO: TLK, KP-4337: refactor this somehow when snapshots will be made portable
    public class SnapshotManager : ISnapshotManager
    {
        private readonly ISnapshottingPolicy snapshottingPolicy;
        private readonly IDomainRepository repository;
        private readonly ISnapshotStore snapshotStore;

        public SnapshotManager(ISnapshottingPolicy snapshottingPolicy, IDomainRepository repository, ISnapshotStore snapshotStore)
        {
            this.snapshottingPolicy = snapshottingPolicy;
            this.repository = repository;
            this.snapshotStore = snapshotStore;
        }

        public void CreateSnapshotIfNeededAndPossible(IAggregateRoot aggregateRoot)
        {
            var aggregate = (AggregateRoot)aggregateRoot; // TODO: TLK, KP-4337: this cast should be removed when snapshots will be made portable

            if (!this.snapshottingPolicy.ShouldCreateSnapshot(aggregate))
                return;

            var snapshot = this.repository.TryTakeSnapshot(aggregate);

            if (snapshot == null)
                return;

            this.snapshotStore.SaveShapshot(snapshot);
        }
    }
}