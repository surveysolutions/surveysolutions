using Ncqrs.Eventing.Storage;

namespace WB.Core.Infrastructure.Implementation.Storage
{
    public class InMemorySnapshotStoreWithCache : InMemoryCachedSnapshotStore, ISnapshotStoreWithCache
    {
        public void CleanCache()
        {
            snapshots.Clear();
            list.Clear();
        }
    }
}