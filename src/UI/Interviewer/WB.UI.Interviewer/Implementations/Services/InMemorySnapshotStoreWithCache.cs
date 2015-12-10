using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Implementation.Storage;

namespace WB.UI.Interviewer.Implementations.Services
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