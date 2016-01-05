using EventStore.ClientAPI;

namespace WB.Infrastructure.Native.Storage.EventStore
{
    public interface IEventStoreConnectionProvider
    {
        IEventStoreConnection Open();
    }
}