using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.Gms.Nearby.Connection;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.UI.Shared.Enumerator.OfflineSync.Entities;
using Payload = WB.UI.Shared.Enumerator.OfflineSync.Services.Entities.Payload;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation
{
    [ExcludeFromCodeCoverage]
    public class NearbyConnection : INearbyConnection
    {
        private readonly INearbyCommunicator communicator;

        private GoogleApiClient api;

        public NearbyConnection(INearbyCommunicator communicator)
        {
            this.communicator = communicator;
        }

        public void SetGoogleApiClient(GoogleApiClient apiClient)
        {
            this.api = apiClient;
        }

        public Task StartDiscovery(string serviceName, Action<string, NearbyDiscoveredEndpointInfo> foundEndpoint, Action<string> lostEndpoint)
        {
            return NearbyClass.Connections.StartDiscoveryAsync(api, serviceName,
                new OnDiscoveryCallback(foundEndpoint, lostEndpoint), new DiscoveryOptions(Strategy.P2pStar));
        }

        public async Task<string> StartAdvertising(string serviceName, string name, NearbyConnectionLifeCycleCallback lifeCycleCallback)
        {
            var result = await NearbyClass.Connections.StartAdvertisingAsync(api, name, serviceName,
                new OnConnectionLifecycleCallback(lifeCycleCallback), new AdvertisingOptions(Strategy.P2pStar));
            return result.LocalEndpointName;
        }

        public Task RequestConnection(string name, string endpoint, NearbyConnectionLifeCycleCallback lifeCycleCallback)
        {
            System.Diagnostics.Debug.WriteLine($"RequestConnection. {name} => {endpoint}");
            return NearbyClass.Connections.RequestConnectionAsync(api, name, endpoint,
                new OnConnectionLifecycleCallback(lifeCycleCallback));
        }

        public Task AcceptConnection(string endpoint)
        {
            System.Diagnostics.Debug.WriteLine($"AcceptConnection. {endpoint}");
            return NearbyClass.Connections.AcceptConnectionAsync(api, endpoint,
                new OnPayloadCallback(new NearbyPayloadCallback(OnPayloadReceived, OnPayloadTransferUpdate)));
        }

        private void OnPayloadTransferUpdate(string endpoint, NearbyPayloadTransferUpdate update)
        {
            System.Diagnostics.Debug.WriteLine($"OnPayloadTransferUpdate. PayloadId: {update.Id}. Transfer ({update.Status.ToString()}) {update.BytesTransferred} of {update.TotalBytes}");
            this.communicator.RecievePayloadTransferUpdate(this, endpoint, update);
        }

        private async void OnPayloadReceived(string endpoint, IPayload payload)
        {
            System.Diagnostics.Debug.WriteLine($"OnPayloadReceived. PayloadId: {payload.Id}");
            await this.communicator.RecievePayloadAsync(this, endpoint, payload);
        }

        public Task RejectConnection(string endpoint)
        {
            return NearbyClass.Connections.RejectConnectionAsync(api, endpoint);
        }

        public Task SendPayloadAsync(string to, IPayload payload)
        {
            var send = payload as Payload;
            System.Diagnostics.Debug.WriteLine($"SendPayloadAsync. PayloadId: {send.Id}");
            return NearbyClass.Connections.SendPayloadAsync(api, to, send.NearbyPayload);
        }
    }
}
