using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.Gms.Nearby.Connection;
using WB.Core.GenericSubdomains.Portable;
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

        private void LogNonSuccesfulResult(Statuses result, ActionArgs args, [CallerMemberName] string method = null)
        {
            if (!result.IsSuccess)
            {
                var nearbyStatus = result.ToConnectionStatus();

                (string key, string data)[] FromStatuses(Statuses status)
                {
                    return new[]
                    {
                        ("method", method),
                        ("StatusCode", status.StatusCode.ToString()),
                        ("StatusMessage", status.StatusMessage),
                        ("StatusCodeName", nearbyStatus.Status.ToString()),
                        ("HasResolution", status.HasResolution.ToString()),
                        ("ResolutionPackage", status.HasResolution ? result.Resolution.CreatorPackage : "no"),
                        ("IsCanceled", status.IsCanceled.ToString()),
                        ("IsInterrupted", status.IsInterrupted.ToString()),
                        ("ConnectionStatusCode", status.Status.ToString())
                    };
                }

                var exception = new Exception();

                foreach (var exceptionData in args.Data.Concat(FromStatuses(result)))
                {
                    exception.Data[exceptionData.key] = exceptionData.data;
                }

                logger.Error("Non successful result", exception);
            }
        }

        private class ActionArgs
        {
            public (string key, string data)[] Data { get; }

            public ActionArgs(params (string key, string data)[] data)
            {
                this.Data = data;
            }
        }

        public Task<NearbyStatus> StartDiscovery(string serviceName)
        {
            Debug.WriteLine("Start discovery for " + serviceName);
            return NearbyClass.Connections.StartDiscoveryAsync(api, serviceName,
                    new OnDiscoveryCallback(FoundEndpoint, LostEndpoint),
                    new DiscoveryOptions(Strategy.P2pStar))
                .ToConnectionStatus(s => LogNonSuccesfulResult(s,
                    new ActionArgs((nameof(serviceName), serviceName))));
        }

        public void StopAllEndpoint()
        {
            Debug.WriteLine("StopAllEndpoints");
            if(api.IsConnected)
                NearbyClass.Connections.StopAllEndpoints(api);
        }

        public async Task<string> StartAdvertising(string serviceName, string name)
        {
            Debug.WriteLine("Start advertising for " + serviceName + " as " + name);

            var result = await NearbyClass.Connections.StartAdvertisingAsync(api, name, serviceName,
                new OnConnectionLifecycleCallback(
                    new NearbyConnectionLifeCycleCallback(OnInitiatedConnection, OnConnectionResult, OnDisconnected)),
                new AdvertisingOptions(Strategy.P2pStar));

            LogNonSuccesfulResult(result.Status,
                new ActionArgs((nameof(serviceName), serviceName),
                    (nameof(name), name)));

            if (!result.Status.IsSuccess)
            {
                var status = result.Status.ToConnectionStatus();
                throw new NearbyConnectionException("Failed to start advertising. " + status.StatusMessage, status.Status);
            }

            return result.LocalEndpointName;
        }

        private readonly NamedLocker locker = new NamedLocker();

        public async Task<NearbyStatus> RequestConnection(string name, string endpoint)
        {
            return await locker.RunWithLock(endpoint, async () =>
            {
                async Task<NearbyStatus> RequestConnectionInternal()
                {
                    Debug.WriteLine($"RequestConnection internal. {name} at {endpoint}");
                    var nearbyStatus = await NearbyClass.Connections.RequestConnectionAsync(api, name, endpoint,
                            new OnConnectionLifecycleCallback(
                                new NearbyConnectionLifeCycleCallback(
                                    OnInitiatedConnection,
                                    OnConnectionResult,
                                    OnDisconnected)))
                        .ToConnectionStatus(status =>
                            LogNonSuccesfulResult(status,
                                new ActionArgs(("name", name), ("endpoint", endpoint))));

                    Debug.WriteLine($"RequestConnection internal. {name} at {endpoint}. Result {nearbyStatus.Status.ToString()}");

                    return nearbyStatus;
                }

                if (RemoteEndpoints.Any(re => re.Enpoint == endpoint))
                {
                    return new NearbyStatus
                    {
                        IsSuccess = true,
                        Status = ConnectionStatusCode.StatusOk
                    };
                }

                Trace($"RequestConnection. {name} => {endpoint}");

                var result = await RequestConnectionInternal();

                if (result.Status == ConnectionStatusCode.StatusBluetoothError)
                {
                    var mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                    if (mBluetoothAdapter.IsEnabled)
                    {
                        mBluetoothAdapter.Disable();

                        result = await RequestConnectionInternal();
                    }
                }

                if (result.Status == ConnectionStatusCode.StatusAlreadyConnectedToEndpoint)
                {
                    this.OnConnectionResult(endpoint, new NearbyConnectionResolution { IsSuccess = true });
                }

                return result;
            });
        }

        public Task<NearbyStatus> AcceptConnection(string endpoint)
        {
            Trace($"AcceptConnection. {endpoint}");

            return NearbyClass.Connections.AcceptConnectionAsync(api, endpoint,
                new OnPayloadCallback(new NearbyPayloadCallback(OnPayloadReceived, OnPayloadTransferUpdate)))
                .ToConnectionStatus(status =>
                    LogNonSuccesfulResult(status, new ActionArgs(("endpoint", endpoint))));
        }

        public Task<NearbyStatus> RejectConnection(string endpoint)
        {
            return NearbyClass.Connections.RejectConnectionAsync(api, endpoint)
                .ToConnectionStatus(status => LogNonSuccesfulResult(status,
                    new ActionArgs(("endpoint", endpoint))));
        }

        public Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload)
        {
            if (payload is Payload send)
            {
                Trace($"SendPayloadAsync. PayloadId: {send.Id}");

                return NearbyClass.Connections.SendPayloadAsync(api, to, send.NearbyPayload)
                    .ToConnectionStatus(status =>
                        LogNonSuccesfulResult(status, new ActionArgs(("to", to))));
            }

            throw new NotImplementedException("Cannot handle payload of type: " + payload.GetType().FullName);
        }

        public IObservable<INearbyEvent> Events { get; }

        public ObservableCollection<RemoteEndpoint> RemoteEndpoints { get; } = new ObservableCollection<RemoteEndpoint>();

        public void StopDiscovery()
        {
            Trace("Stop discovery");
            if (api.IsConnected)
                NearbyClass.Connections.StopDiscovery(api);
        }

        public void StopAdvertising()
        {
            Trace("Stop advertising");
            if (api.IsConnected)
                NearbyClass.Connections.StopAdvertising(api);
        }

        public void StopAll()
        {
            try
            {
                this.StopAdvertising();
                this.StopDiscovery();
                this.StopAllEndpoint();
                this.RemoteEndpoints.Clear();
                this.knownEnpoints.Clear();
            }
            catch { /* om om om */}
        }

        public void SetGoogleApiClient(GoogleApiClient apiClient)
        {
            Trace("Google API SET");
            api = apiClient;
        }

        private void LostEndpoint(string endpoint)
        {
            if(knownEnpoints.TryRemove(endpoint, out var endpointInfo)) {

                Trace($"Lost endpoint: {endpoint}. Name: {endpointInfo ?? "<unknown>"}");
                events.OnNext(new NearbyEvent.EndpointLost(endpoint));
            }
        }

        private void FoundEndpoint(string endpoint, NearbyDiscoveredEndpointInfo endpointInfo)
        {
            if (knownEnpoints.TryAdd(endpoint, endpointInfo.EndpointName))
            {
                Trace($"Lost endpoint: {endpoint}. Name: {endpointInfo.EndpointName}");
                events.OnNext(new NearbyEvent.EndpointFound(endpoint, endpointInfo));
            }
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
                this.RemoteEndpoints.Add(new RemoteEndpoint { Enpoint = endpoint, Name = name });
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
