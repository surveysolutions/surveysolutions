using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Snapshots
{
    public interface ISnapshooter
    {
        void CreateSnapshotIfNeededAndPossible(IAggregateRoot aggregateRoot);
    }
}