using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Snapshots
{
    // TODO: TLK, KP-4337: refactor this somehow when snapshots will be made portable
    public interface ISnapshotManager
    {
        void CreateSnapshotIfNeededAndPossible(IAggregateRoot aggregateRoot);
    }
}