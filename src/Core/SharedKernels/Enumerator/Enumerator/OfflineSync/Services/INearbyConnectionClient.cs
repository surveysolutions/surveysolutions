using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface INearbyConnectionClient
    {
        Task<NearbyStatus> StartDiscoveryAsync(string serviceName, CancellationToken cancellationToken);
        Task<string> StartAdvertisingAsync(string serviceName, string name, CancellationToken cancellationToken);
        Task<NearbyStatus> RequestConnectionAsync(string name, string endpoint, CancellationToken cancellationToken);
        Task<NearbyStatus> AcceptConnectionAsync(string endpoint, CancellationToken cancellationToken);
        Task<NearbyStatus> RejectConnectionAsync(string endpoint, CancellationToken cancellationToken);
        Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload, CancellationToken cancellationToken);

        void StopAllEndpoint();
        void StopDiscovery();
        void StopAdvertising();
        void StopAll();

        event EventHandler<string> LostEndpoint;
        event EventHandler<NearbyDiscoveredEndpointInfo> FoundEndpoint;
        event EventHandler<string> Disconnected;
        event EventHandler<NearbyConnectionResolution> ConnectionResult;
        event EventHandler<NearbyConnectionInfo> InitiatedConnection;
        event EventHandler<IPayload> PayloadReceived;
        event EventHandler<NearbyPayloadTransferUpdate> PayloadTransferUpdate;
    }
}
