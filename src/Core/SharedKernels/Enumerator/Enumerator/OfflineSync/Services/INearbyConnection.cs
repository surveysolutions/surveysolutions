using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface INearbyConnection
    {
        Task<NearbyStatus> StartDiscovery(string serviceName);
        Task<string> StartAdvertising(string serviceName, string name);
        Task<NearbyStatus> RequestConnection(string name, string endpoint);
        Task<NearbyStatus> AcceptConnection(string endpoint);
        Task<NearbyStatus> RejectConnection(string endpoint);
        Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload);
        void StopAllEndpoint();
        IObservable<INearbyEvent> Events { get; }
        ObservableCollection<RemoteEndpoint> RemoteEndpoints { get; }
        void StopDiscovery();
        void StopAdvertising();
        void StopAll();
    }
}
