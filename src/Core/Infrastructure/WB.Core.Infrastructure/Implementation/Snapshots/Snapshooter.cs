using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Snapshots;

namespace WB.Core.Infrastructure.Implementation.Snapshots
{
    internal class Snapshooter : ISnapshooter
    {
        private readonly ISnapshottingPolicy snapshottingPolicy;
        private readonly IDomainRepository repository;
        private readonly ISnapshotStore snapshotStore;

        public Snapshooter(ISnapshottingPolicy snapshottingPolicy, IDomainRepository repository, ISnapshotStore snapshotStore)
        {
            this.snapshottingPolicy = snapshottingPolicy;
            this.repository = repository;
            this.snapshotStore = snapshotStore;
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
    }
}