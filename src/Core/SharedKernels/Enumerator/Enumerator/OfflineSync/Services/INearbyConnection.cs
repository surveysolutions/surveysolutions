using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface INearbyConnection
    {
        Task<NearbyStatus> StartDiscoveryAsync(string serviceName, CancellationToken cancellationToken);
        Task<string> StartAdvertisingAsync(string serviceName, string name, CancellationToken cancellationToken);
        Task<NearbyStatus> RequestConnectionAsync(string name, string endpoint, CancellationToken cancellationToken);
        Task<NearbyStatus> AcceptConnectionAsync(string endpoint, CancellationToken cancellationToken);
        Task<NearbyStatus> RejectConnectionAsync(string endpoint, CancellationToken cancellationToken);
        Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload, CancellationToken cancellationToken);
        void StopAllEndpoint();
        IObservable<INearbyEvent> Events { get; }
        ObservableCollection<RemoteEndpoint> RemoteEndpoints { get; }
        void StopDiscovery();
        void StopAdvertising();
        void StopAll();
    }
}
