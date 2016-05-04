using System.Threading.Tasks;

namespace WB.Infrastructure.Native.Storage.EventStore
{
    public interface IEventStoreApiService
    {
        Task RunScavengeAsync();
    }
}