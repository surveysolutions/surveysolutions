namespace Ncqrs.Eventing.Storage
{
    public interface ISnapshotStoreWithCache : ISnapshotStore
    {
        void CleanCache();
    }
}
