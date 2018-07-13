using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.Gms.Nearby.Connection;
using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly ILogger logger;
        private readonly Subject<INearbyEvent> events;

        private GoogleApiClient api;

        public NearbyConnection(INearbyCommunicator communicator, ILogger logger)
        {
            this.communicator = communicator;
            this.logger = logger;
            events = new Subject<INearbyEvent>();
            Events = events;
        }

        private void Trace(string message)
        {
            logger.Info(message);
            Debug.WriteLine("NearbyConnection - " + message);
        }

        public Task StartDiscovery(string serviceName)
        {
            return NearbyClass.Connections.StartDiscoveryAsync(api, serviceName,
                new OnDiscoveryCallback(FoundEndpoint, LostEndpoint),
                new DiscoveryOptions(Strategy.P2pStar));
        }

        public async Task<string> StartAdvertising(string serviceName, string name)
        {
            var result = await NearbyClass.Connections.StartAdvertisingAsync(api,  name, serviceName,
                new OnConnectionLifecycleCallback(
                    new NearbyConnectionLifeCycleCallback(OnInitiatedConnection, OnConnectionResult, OnDisconnected)),
                new AdvertisingOptions(Strategy.P2pStar));
            return result.LocalEndpointName;
        }

        public Task RequestConnection(string name, string endpoint)
        {
            Trace($"RequestConnection. {name} => {endpoint}");

            return NearbyClass.Connections.RequestConnectionAsync(api, name, endpoint,
                new OnConnectionLifecycleCallback(
                    new NearbyConnectionLifeCycleCallback(OnInitiatedConnection, OnConnectionResult, OnDisconnected)));
        }

        public Task AcceptConnection(string endpoint)
        {
            Trace($"AcceptConnection. {endpoint}");

            return NearbyClass.Connections.AcceptConnectionAsync(api, endpoint,
                new OnPayloadCallback(new NearbyPayloadCallback(OnPayloadReceived, OnPayloadTransferUpdate)));
        }

        public Task RejectConnection(string endpoint)
        {
            return NearbyClass.Connections.RejectConnectionAsync(api, endpoint);
        }

        public Task SendPayloadAsync(string to, IPayload payload)
        {
            var send = payload as Payload;
            Trace($"SendPayloadAsync. PayloadId: {send?.Id ?? -1}");
            return NearbyClass.Connections.SendPayloadAsync(api, to, send.NearbyPayload);
        }

        public IObservable<INearbyEvent> Events { get; }

        public ObservableCollection<RemoteEndpoint> RemoteEndpoints { get; } = new ObservableCollection<RemoteEndpoint>();

        public Task StopDiscovery()
        {
            Trace("Stop discovery");
            NearbyClass.Connections.StopDiscovery(api);
            return Task.CompletedTask;
        }

        public Task StopAdvertising()
        {
            Trace("Stop advertising");
            NearbyClass.Connections.StopAdvertising(api);
            return Task.CompletedTask;
        }

        public void SetGoogleApiClient(GoogleApiClient apiClient)
        {
            Trace("Google API SET");
            api = apiClient;
        }

        private void LostEndpoint(string endpoint)
        {
            knownEnpoints.TryRemove(endpoint, out var endpointInfo);

            Trace($"Lost endpoint: {endpoint}. Name: {endpointInfo ?? "<unknown>"}");
            events.OnNext(new NearbyEvent.EndpointLost(endpoint));
        }

        private void FoundEndpoint(string endpoint, NearbyDiscoveredEndpointInfo endpointInfo)
        {
            knownEnpoints.TryAdd(endpoint, endpointInfo.EndpointName);
            Trace($"Lost endpoint: {endpoint}. Name: {endpointInfo.EndpointName}");
            events.OnNext(new NearbyEvent.EndpointFound(endpoint, endpointInfo));
        }

        protected virtual void OnDisconnected(string endpoint)
        {
            var exising = this.RemoteEndpoints.FirstOrDefault(re => re.Enpoint == endpoint);
            events.OnNext(new NearbyEvent.Disconnected(endpoint, exising?.Name));

            Trace($"Disconnected from endpoint: {endpoint}. Name: {exising?.Name ?? "<unknown>"}");

            if (exising != null)
            {
                this.RemoteEndpoints.Remove(exising);
            }
        }

        private void OnConnectionResult(string endpoint, NearbyConnectionResolution resolution)
        {
            if (resolution.IsSuccess)
            {
                var exising = this.RemoteEndpoints.FirstOrDefault(re => re.Enpoint == endpoint);

                if (exising != null)
                {
                    this.RemoteEndpoints.Remove(exising);
                }

                knownEnpoints.TryGetValue(endpoint, out var name);

                Trace($"Connected to endpoint: {endpoint}. Name: {name}");
                this.RemoteEndpoints.Add(new RemoteEndpoint{ Enpoint = endpoint, Name = name});
                events.OnNext(new NearbyEvent.Connected(endpoint, resolution, name));
            }
        }

        private readonly ConcurrentDictionary<string, string> knownEnpoints = new ConcurrentDictionary<string, string>();

        private void OnInitiatedConnection(string endpoint, NearbyConnectionInfo info)
        {
            knownEnpoints.TryAdd(endpoint, info.EndpointName);
            events.OnNext(new NearbyEvent.InitiatedConnection(endpoint, info));
            Trace($"Initiated connection from endpoint: {endpoint}. Name: {info.EndpointName}");
        }

        private void OnPayloadTransferUpdate(string endpoint, NearbyPayloadTransferUpdate update)
        {
            Trace($"Payload transfer update {endpoint}. PayloadId: {update.Id}. Transfer ({update.Status.ToString()}) {update.BytesTransferred} of {update.TotalBytes}");
            communicator.RecievePayloadTransferUpdate(this, endpoint, update);
        }

        private async void OnPayloadReceived(string endpoint, IPayload payload)
        {
            Trace($"OnPayloadReceived. PayloadId: {payload.Id}");

            await communicator.RecievePayloadAsync(this, endpoint, payload);
        }
    }
}
