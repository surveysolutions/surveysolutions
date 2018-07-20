using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.Gms.Nearby.Connection;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Utils;
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
        private CancellationTokenSource cts;
        private GoogleApiClient api;

        public NearbyConnection(INearbyCommunicator communicator, ILogger logger)
        {
            this.communicator = communicator;
            this.logger = logger;
            events = new Subject<INearbyEvent>();
            Events = events;
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
            this.logger.Verbose($"({serviceName}) ENTER");

            cts = new CancellationTokenSource();

            var result = await NearbyClass.Connections.StartDiscoveryAsync(api, serviceName,
                    new OnDiscoveryCallback(FoundEndpoint, LostEndpoint),
                    new DiscoveryOptions(Strategy.P2pStar))
                .ToConnectionStatus(s => LogNonSuccesfulResult(s,
                    new ActionArgs((nameof(serviceName), serviceName))));

            this.logger.Verbose($"({serviceName}) EXIT => {result.Status.ToString()}");
            return result;
        }

        public void StopAllEndpoint()
        {
            if (api.IsConnected)
            {
                this.logger.Verbose();
                NearbyClass.Connections.StopAllEndpoints(api);
            }
        }

        public async Task<string> StartAdvertising(string serviceName, string name)
        {
            this.logger.Verbose($"{serviceName}, {name}) ENTER");
            cts = new CancellationTokenSource();

            var result = await NearbyClass.Connections.StartAdvertisingAsync(api, name, serviceName,
                    new OnConnectionLifecycleCallback(
                        new NearbyConnectionLifeCycleCallback(OnInitiatedConnection, OnConnectionResult, OnDisconnected)),
                    new AdvertisingOptions(Strategy.P2pStar))
                    .AsCancellableTask(cts);

            LogNonSuccesfulResult(result.Status,
                new ActionArgs((nameof(serviceName), serviceName),
                    (nameof(name), name)));

            if (!result.Status.IsSuccess)
            {
                var status = result.Status.ToConnectionStatus();
                throw new NearbyConnectionException("Failed to start advertising. " + status.StatusMessage, status.Status);
            }

            this.logger.Verbose($"{serviceName}, {name}) EXIT => {result.Status.ToConnectionStatus().Status.ToString()}");
            return result.LocalEndpointName;
        }

        private NamedAsyncLocker locker = new NamedAsyncLocker();
        private readonly ConcurrentDictionary<string, bool> pendingRequestConnections = new ConcurrentDictionary<string, bool>();

        public async Task<NearbyStatus> RequestConnection(string name, string endpoint)
        {
            this.logger.Verbose($"({name}, {endpoint}) ENTER. Waiting lock");

            if (cts.IsCancellationRequested)
                return new NearbyStatus
                {
                    IsCanceled = true,
                    IsSuccess = false
                };

            return await locker.RunWithLockAsync(endpoint, async () =>
            {
                this.logger.Verbose($"({name}, {endpoint}) ENTER. Done waiting lock");

                // if there is already succesfull pending request. Than do nothing
                if (pendingRequestConnections.TryGetValue(endpoint, out _))
                {
                    this.logger.Verbose($"({ name}, { endpoint}) EXIT. There is already pending requests");

                    return new NearbyStatus
                    {
                        IsSuccess = true,
                        Status = ConnectionStatusCode.StatusOk
                    };
                }

                async Task<NearbyStatus> RequestConnectionInternal()
                {
                    try
                    {
                        this.logger.Verbose($"({name}, {endpoint}) EXECUTE");
                        var nearbyStatus = await NearbyClass.Connections.RequestConnectionAsync(api, name, endpoint,
                                new OnConnectionLifecycleCallback(
                                    new NearbyConnectionLifeCycleCallback(
                                        OnInitiatedConnection,
                                        OnConnectionResult,
                                        OnDisconnected))).AsCancellableTask(cts)
                            .ToConnectionStatus(status =>
                                LogNonSuccesfulResult(status,
                                    new ActionArgs(("name", name), ("endpoint", endpoint))));

                        this.logger.Verbose($"({name}, {endpoint}) EXECUTE DONE => {nearbyStatus.Status.ToString()}");
                        return nearbyStatus;
                    }
                    catch (Exception)
                    {
                        return new NearbyStatus
                        {
                            IsCanceled = true,
                            IsSuccess = false,
                            Status = ConnectionStatusCode.StatusError
                        };
                    }
                }

                if (RemoteEndpoints.Any(re => re.Enpoint == endpoint))
                {
                    this.logger.Verbose($"({name}, {endpoint}) Already connected. EXIT");
                    return new NearbyStatus
                    {
                        IsSuccess = true,
                        Status = ConnectionStatusCode.StatusOk
                    };
                }

                var result = await RequestConnectionInternal();

                this.logger.Verbose($"({name}, {endpoint}) RequestConnectionInternal ended with status: {result.Status}. IsSuccess = {result.IsSuccess}.");

                if (result.Status == ConnectionStatusCode.StatusBluetoothError)
                {
                    result = await RequestConnectionInternal();
                }

                if (result.Status == ConnectionStatusCode.StatusAlreadyConnectedToEndpoint)
                {
                    this.OnConnectionResult(endpoint, new NearbyConnectionResolution { IsSuccess = true });
                }

                if (result.IsSuccess)
                {
                    this.logger.Verbose($"({name}, {endpoint}) Added pending request connection.");
                    pendingRequestConnections.TryAdd(endpoint, true);
                }

                this.logger.Verbose($"({name}, {endpoint}) => {result.Status}");
                return result;
            });
        }

        public async Task<NearbyStatus> AcceptConnection(string endpoint)
        {
            this.logger.Verbose($"({endpoint}) ENTER");
            var result = await NearbyClass.Connections.AcceptConnectionAsync(api, endpoint,
                new OnPayloadCallback(new NearbyPayloadCallback(OnPayloadReceived, OnPayloadTransferUpdate)))
                .ToConnectionStatus(status =>
                    LogNonSuccesfulResult(status, new ActionArgs(("endpoint", endpoint))));

            this.logger.Verbose($"({endpoint}) EXIT => {result.Status.ToString()}");
            return result;
        }

        public async Task<NearbyStatus> RejectConnection(string endpoint)
        {
            this.logger.Verbose($"({endpoint}) ENTER");

            var result = await NearbyClass.Connections.RejectConnectionAsync(api, endpoint)
                .ToConnectionStatus(status => LogNonSuccesfulResult(status,
                    new ActionArgs(("endpoint", endpoint))));

            this.logger.Verbose($"({endpoint}) EXIT => {result.Status.ToString()}");

            return result;
        }

        public async Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload)
        {
            this.logger.Verbose($"({to}, '{payload}') ENTER");
            if (payload is Payload send)
            {
                var result = await NearbyClass.Connections.SendPayloadAsync(api, to, send.NearbyPayload)
                    .ToConnectionStatus(status =>
                        LogNonSuccesfulResult(status, new ActionArgs(("to", to))));

                this.logger.Verbose($"({to}, '{payload}') EXIT => {result.Status.ToString()}");
                return result;
            }

            throw new NotImplementedException("Cannot handle payload of type: " + payload.GetType().FullName);
        }

        public IObservable<INearbyEvent> Events { get; }

        public ObservableCollection<RemoteEndpoint> RemoteEndpoints { get; } = new ObservableCollection<RemoteEndpoint>();

        public void StopDiscovery()
        {
            this.logger.Verbose();

            this.cts?.Cancel();
            this.cts = null;

            if (api.IsConnected)
            {
                this.logger.Verbose("EXECUTE");
                NearbyClass.Connections.StopDiscovery(api);
            }
        }

        public void StopAdvertising()
        {
            this.logger.Verbose();
            this.cts?.Cancel();
            this.cts = null;
            if (api.IsConnected)
            {
                this.logger.Verbose("EXECUTE");
                NearbyClass.Connections.StopAdvertising(api);
            }
        }

        public void StopAll()
        {
            this.logger.Verbose();
            try
            {
                this.StopAdvertising();
                this.StopDiscovery();
                this.StopAllEndpoint();
                this.RemoteEndpoints.Clear();
                this.knownEnpoints.Clear();
                this.pendingRequestConnections.Clear();
                this.locker = new NamedAsyncLocker();
            }
            catch { /* om om om */}
        }

        public void SetGoogleApiClient(GoogleApiClient apiClient)
        {
            this.logger.Verbose();
            api = apiClient;
        }

        private void LostEndpoint(string endpoint)
        {
            this.logger.Verbose($"({endpoint}) ENTER");
            
            if (knownEnpoints.TryRemove(endpoint, out var name))
            {
                this.logger.Verbose($"({endpoint}) Remove known: '{name ?? "<unknown>"}'. Notify.");
                events.OnNext(new NearbyEvent.EndpointLost(endpoint));
            }
        }

        private void FoundEndpoint(string endpoint, NearbyDiscoveredEndpointInfo endpointInfo)
        {
            this.logger.Verbose($"({endpoint}, {endpointInfo.EndpointName}) ENTER");
            
            if (knownEnpoints.TryAdd(endpoint, endpointInfo.EndpointName))
            {
                this.logger.Verbose($"({endpoint}) Add known: '{endpointInfo.EndpointName ?? "<unknown>"}'. Notify.");
                events.OnNext(new NearbyEvent.EndpointFound(endpoint, endpointInfo));
            }
        }

        protected virtual void OnDisconnected(string endpoint)
        {
            this.logger.Verbose($"({endpoint}) ENTER");

            var exising = this.RemoteEndpoints.FirstOrDefault(re => re.Enpoint == endpoint);
            events.OnNext(new NearbyEvent.Disconnected(endpoint, exising?.Name));

            if (exising != null)
            {
                this.RemoteEndpoints.Remove(exising);
            }
        }

        private void OnConnectionResult(string endpoint, NearbyConnectionResolution resolution)
        {
            this.logger.Verbose($"({endpoint}, {resolution.IsSuccess}) ENTER. Remove pending request");
            pendingRequestConnections.TryRemove(endpoint, out _);
            this.cts?.Token.ThrowIfCancellationRequested();

            if (resolution.IsSuccess)
            {
                var exising = this.RemoteEndpoints.FirstOrDefault(re => re.Enpoint == endpoint);

                if (exising != null)
                {
                    this.RemoteEndpoints.Remove(exising);
                }

                knownEnpoints.TryGetValue(endpoint, out var name);

                this.logger.Verbose($"Connected to endpoint: {endpoint}. Name: {name}");
                this.RemoteEndpoints.Add(new RemoteEndpoint { Enpoint = endpoint, Name = name });
                events.OnNext(new NearbyEvent.Connected(endpoint, resolution, name));
            }
        }

        private readonly ConcurrentDictionary<string, string> knownEnpoints = new ConcurrentDictionary<string, string>();

        private void OnInitiatedConnection(string endpoint, NearbyConnectionInfo info)
        {
            this.logger.Verbose($"({endpoint}, name: {info.EndpointName}, incoming: {info.IsIncomingConnection}, auth: {info.AuthenticationToken})");
            knownEnpoints.TryAdd(endpoint, info.EndpointName);
            events.OnNext(new NearbyEvent.InitiatedConnection(endpoint, info));
        }

        private void OnPayloadTransferUpdate(string endpoint, NearbyPayloadTransferUpdate update)
        {
            this.logger.Verbose($"({endpoint}, payloadId: {update.Id}, status: {update.Status.ToString()}, {update.BytesTransferred} of {update.TotalBytes}");
            communicator.RecievePayloadTransferUpdate(this, endpoint, update);
        }

        private async void OnPayloadReceived(string endpoint, IPayload payload)
        {
            this.logger.Verbose($"({endpoint}, {payload.ToString()})");
            await communicator.RecievePayloadAsync(this, endpoint, payload);
        }
    }
}
