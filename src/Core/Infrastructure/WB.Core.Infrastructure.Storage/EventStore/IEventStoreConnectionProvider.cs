using EventStore.ClientAPI;

namespace WB.Core.Infrastructure.Storage.EventStore
{
    public interface IEventStoreConnectionProvider
    {
        IEventStoreConnection Open();
    }
}