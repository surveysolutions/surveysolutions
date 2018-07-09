using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Subjects;
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
        private readonly Subject<INearbyEvent> events;

        private GoogleApiClient api;

        public NearbyConnection(INearbyCommunicator communicator)
        {
            this.communicator = communicator;
            events = new Subject<INearbyEvent>();
            Events = events;
        }

        public Task StartDiscovery(string serviceName)
        {
            return NearbyClass.Connections.StartDiscoveryAsync(api, serviceName,
                new OnDiscoveryCallback(FoundEndpoint, LostEndpoint),
                new DiscoveryOptions(Strategy.P2pStar));
        }

        public async Task<string> StartAdvertising(string serviceName, string name)
        {
            var result = await NearbyClass.Connections.StartAdvertisingAsync(api, name, serviceName,
                new OnConnectionLifecycleCallback(
                    new NearbyConnectionLifeCycleCallback(OnInitiatedConnection, OnConnectionResult, OnDisconnected)),
                new AdvertisingOptions(Strategy.P2pStar));
            return result.LocalEndpointName;
        }

        public Task RequestConnection(string name, string endpoint)
        {
            Debug.WriteLine($"RequestConnection. {name} => {endpoint}");

            return NearbyClass.Connections.RequestConnectionAsync(api, name, endpoint,
                new OnConnectionLifecycleCallback(
                    new NearbyConnectionLifeCycleCallback(OnInitiatedConnection, OnConnectionResult, OnDisconnected)));
        }

        public Task AcceptConnection(string endpoint)
        {
            Debug.WriteLine($"AcceptConnection. {endpoint}");

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
            Debug.WriteLine($"SendPayloadAsync. PayloadId: {send.Id}");
            return NearbyClass.Connections.SendPayloadAsync(api, to, send.NearbyPayload);
        }

        public IObservable<INearbyEvent> Events { get; }

        public ObservableCollection<RemoteEndpoint> RemoteEndpoints { get; } = new ObservableCollection<RemoteEndpoint>();

        public Task StopDiscovery()
        {
            NearbyClass.Connections.StopDiscovery(api);
            return Task.CompletedTask;
        }

        public Task StopAdvertising()
        {
            NearbyClass.Connections.StopAdvertising(api);
            return Task.CompletedTask;
        }

        public void SetGoogleApiClient(GoogleApiClient apiClient)
        {
            api = apiClient;
        }

        private void LostEndpoint(string endpoint)
        {
            knownEnpoints.TryRemove(endpoint, out _);
            events.OnNext(new NearbyEvent.EndpointLost(endpoint));
        }

        private void FoundEndpoint(string endpoint, NearbyDiscoveredEndpointInfo endpointInfo)
        {
            knownEnpoints.TryAdd(endpoint, endpointInfo.EndpointName);
            events.OnNext(new NearbyEvent.EndpointFound(endpoint, endpointInfo));
        }

        protected virtual void OnDisconnected(string endpoint)
        {
            events.OnNext(new NearbyEvent.Disconnected(endpoint));

            var exising = this.RemoteEndpoints.FirstOrDefault(re => re.Enpoint == endpoint);

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

                this.RemoteEndpoints.Add(new RemoteEndpoint{ Enpoint = endpoint, Name = name});
            }

            events.OnNext(new NearbyEvent.Connected(endpoint, resolution));
        }

        private readonly ConcurrentDictionary<string, string> knownEnpoints = new ConcurrentDictionary<string, string>();

        private void OnInitiatedConnection(string endpoint, NearbyConnectionInfo info)
        {
            knownEnpoints.TryAdd(endpoint, info.EndpointName);
            events.OnNext(new NearbyEvent.InitiatedConnection(endpoint, info));
        }

        private void OnPayloadTransferUpdate(string endpoint, NearbyPayloadTransferUpdate update)
        {
            Debug.WriteLine(
                $"OnPayloadTransferUpdate. PayloadId: {update.Id}. Transfer ({update.Status.ToString()}) {update.BytesTransferred} of {update.TotalBytes}");
            communicator.RecievePayloadTransferUpdate(this, endpoint, update);
        }

        private async void OnPayloadReceived(string endpoint, IPayload payload)
        {
            Debug.WriteLine($"OnPayloadReceived. PayloadId: {payload.Id}");

            await communicator.RecievePayloadAsync(this, endpoint, payload);
        }
    }
}
