namespace WB.Core.Infrastructure.Storage.EventStore
{
    public interface IEventStoreApiService
    {
        void RunScavenge();
    }
}