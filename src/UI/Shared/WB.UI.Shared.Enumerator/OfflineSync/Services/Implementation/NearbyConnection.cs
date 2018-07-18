using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
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

        private void Trace(string message = null, [CallerMemberName] string method = null)
        {
            message = message == null ? "" : ": " + message;
            logger.Trace(method + message);
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

                logger.Error("Connection error", exception);
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

        public async Task<NearbyStatus> StartDiscovery(string serviceName)
        {
            Trace($"({serviceName}) ENTER");
            var result = await NearbyClass.Connections.StartDiscoveryAsync(api, serviceName,
                    new OnDiscoveryCallback(FoundEndpoint, LostEndpoint),
                    new DiscoveryOptions(Strategy.P2pStar))
                .ToConnectionStatus(s => LogNonSuccesfulResult(s,
                    new ActionArgs((nameof(serviceName), serviceName))));
            Trace($"({serviceName}) EXIT => {result.Status.ToString()}");
            return result;
        }

        public void StopAllEndpoint()
        {
            if (api.IsConnected)
            {
                Trace();
                NearbyClass.Connections.StopAllEndpoints(api);
            }
        }

        public async Task<string> StartAdvertising(string serviceName, string name)
        {
            Trace($"{serviceName}, {name}) ENTER");

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

            Trace($"{serviceName}, {name}) EXIT => {result.Status.ToConnectionStatus().Status.ToString()}");
            return result.LocalEndpointName;
        }

        private readonly NamedLocker locker = new NamedLocker();

        public async Task<NearbyStatus> RequestConnection(string name, string endpoint)
        {
            Trace($"({name}, {endpoint}) ENTER. Waiting lock");
            return await locker.RunWithLock(endpoint, async () =>
            {
                Trace($"({name}, {endpoint}) ENTER. Done waiting lock");
                async Task<NearbyStatus> RequestConnectionInternal()
                {
                    Trace($"({name}, {endpoint}) EXECUTE");
                    var nearbyStatus = await NearbyClass.Connections.RequestConnectionAsync(api, name, endpoint,
                            new OnConnectionLifecycleCallback(
                                new NearbyConnectionLifeCycleCallback(
                                    OnInitiatedConnection,
                                    OnConnectionResult,
                                    OnDisconnected)))
                        .ToConnectionStatus(status =>
                            LogNonSuccesfulResult(status,
                                new ActionArgs(("name", name), ("endpoint", endpoint))));

                    Trace($"({name}, {endpoint}) EXECUTE DONE => {nearbyStatus.Status.ToString()}");
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


                var result = await RequestConnectionInternal();

                if (result.Status == ConnectionStatusCode.StatusBluetoothError)
                {
                    Trace($"({name}, {endpoint}) DISABLING BLUETOOTH");
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

                Trace($"({name}, {endpoint}) => {result.Status.ToString()}");
                return result;
            });
        }

        public async Task<NearbyStatus> AcceptConnection(string endpoint)
        {
            Trace($"({endpoint}) ENTER");

            var result = await NearbyClass.Connections.AcceptConnectionAsync(api, endpoint,
                new OnPayloadCallback(new NearbyPayloadCallback(OnPayloadReceived, OnPayloadTransferUpdate)))
                .ToConnectionStatus(status =>
                    LogNonSuccesfulResult(status, new ActionArgs(("endpoint", endpoint))));

            Trace($"({endpoint}) EXIT => {result.Status.ToString()}");
            return result;
        }

        public async Task<NearbyStatus> RejectConnection(string endpoint)
        {
            Trace($"({endpoint}) ENTER");
            
            var result = await NearbyClass.Connections.RejectConnectionAsync(api, endpoint)
                .ToConnectionStatus(status => LogNonSuccesfulResult(status,
                    new ActionArgs(("endpoint", endpoint))));

            Trace($"({endpoint}) EXIT => {result.Status.ToString()}");

            return result;
        }

        public async Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload)
        {
            Trace($"({to}, '{payload}') ENTER");
            if (payload is Payload send)
            {
                var result = await  NearbyClass.Connections.SendPayloadAsync(api, to, send.NearbyPayload)
                    .ToConnectionStatus(status =>
                        LogNonSuccesfulResult(status, new ActionArgs(("to", to))));

                Trace($"({to}, '{payload}') EXIT => {result.Status.ToString()}");
                return result;
            }

            throw new NotImplementedException("Cannot handle payload of type: " + payload.GetType().FullName);
        }

        public IObservable<INearbyEvent> Events { get; }

        public ObservableCollection<RemoteEndpoint> RemoteEndpoints { get; } = new ObservableCollection<RemoteEndpoint>();

        public void StopDiscovery()
        {
            Trace();
            if (api.IsConnected)
            {
                Trace("EXECUTE");
                NearbyClass.Connections.StopDiscovery(api);
            }
        }

        public void StopAdvertising()
        {
            Trace();
            if (api.IsConnected)
            {
                Trace("EXECUTE");
                NearbyClass.Connections.StopAdvertising(api);
            }
        }

        public void StopAll()
        {
            Trace();
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
            Trace();
            api = apiClient;
        }

        private void LostEndpoint(string endpoint)
        {
            Trace($"({endpoint}) ENTER");

            if (knownEnpoints.TryRemove(endpoint, out var name))
            {
                Trace($"({endpoint}) Remove known: '{name ?? "<unknown>"}'. Notify.");
                events.OnNext(new NearbyEvent.EndpointLost(endpoint));
            }
        }

        private void FoundEndpoint(string endpoint, NearbyDiscoveredEndpointInfo endpointInfo)
        {
            Trace($"({endpoint}, {endpointInfo.EndpointName}) ENTER");
            if (knownEnpoints.TryAdd(endpoint, endpointInfo.EndpointName))
            {
                Trace($"({endpoint}) Add known: '{endpointInfo.EndpointName ?? "<unknown>"}'. Notify.");
                events.OnNext(new NearbyEvent.EndpointFound(endpoint, endpointInfo));
            }
        }

        protected virtual void OnDisconnected(string endpoint)
        {
            Trace($"({endpoint}) ENTER");

            var exising = this.RemoteEndpoints.FirstOrDefault(re => re.Enpoint == endpoint);
            events.OnNext(new NearbyEvent.Disconnected(endpoint, exising?.Name));

            if (exising != null)
            {
                this.RemoteEndpoints.Remove(exising);
            }
        }

        private void OnConnectionResult(string endpoint, NearbyConnectionResolution resolution)
        {
            Trace($"({endpoint}, {resolution.IsSuccess}) ENTER");

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
            Trace($"({endpoint}, name: {info.EndpointName}, incoming: {info.IsIncomingConnection}, auth: {info.AuthenticationToken})");
            knownEnpoints.TryAdd(endpoint, info.EndpointName);
            events.OnNext(new NearbyEvent.InitiatedConnection(endpoint, info));
        }

        private void OnPayloadTransferUpdate(string endpoint, NearbyPayloadTransferUpdate update)
        {
            Trace($"({endpoint}, payloadId: {update.Id}, status: {update.Status.ToString()}, {update.BytesTransferred} of {update.TotalBytes}");
            communicator.RecievePayloadTransferUpdate(this, endpoint, update);
        }

        private async void OnPayloadReceived(string endpoint, IPayload payload)
        {
            Trace($"({endpoint}, {payload.ToString()})");
            await communicator.RecievePayloadAsync(this, endpoint, payload);
        }
    }
}
