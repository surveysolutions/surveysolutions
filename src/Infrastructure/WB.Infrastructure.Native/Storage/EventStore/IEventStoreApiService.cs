namespace WB.Infrastructure.Native.Storage.EventStore
{
    public interface IEventStoreApiService
    {
        void RunScavenge();
    }
}