using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.Gms.Nearby.Connection;
using Java.Util.Concurrent;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.UI.Shared.Enumerator.OfflineSync.Entities;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation
{
    public class NearbyConnectionClient : INearbyConnectionClient
    {
        private readonly IGoogleApiClientFactory apiClientFactory;
        private readonly ILogger logger;

        public NearbyConnectionClient(ILogger logger, IGoogleApiClientFactory apiClientFactory)
        {
            this.logger = logger;
            this.apiClientFactory = apiClientFactory;
        }

        private GoogleApiClient Api => apiClientFactory?.GoogleApiClient 
                                       ?? throw new NearbyConnectionException("Not connected to Google API", ConnectionStatusCode.StatusError);

        public event EventHandler<string> LostEndpoint;
        public event EventHandler<NearbyDiscoveredEndpointInfo> FoundEndpoint;
        public event EventHandler<string> Disconnected;
        public event EventHandler<NearbyConnectionResolution> ConnectionResult;
        public event EventHandler<NearbyConnectionInfo> InitiatedConnection;
        public event EventHandler<IPayload> PayloadReceived;
        public event EventHandler<NearbyPayloadTransferUpdate> PayloadTransferUpdate;

        public async Task<NearbyStatus> StartDiscoveryAsync(string serviceName, CancellationToken cancellationToken)
        {
            this.logger.Verbose($"{serviceName} ENTER");

            var result = await ExecuteNearbyActionWithTimeoutAsync(
                NearbyClass.Connections.StartDiscovery(Api, serviceName,
                    new OnDiscoveryCallback(OnFoundEndpoint, OnLostEndpoint, cancellationToken),
                    new DiscoveryOptions(Strategy.P2pStar)))
                .ConfigureAwait(false);

            LogNonSuccesfulResult(result, new ActionArgs((nameof(serviceName), serviceName)));

            this.logger.Verbose($"{serviceName} EXIT. Result: {result.Status}");

            return ToConnectionStatus(result);
        }

        public async Task<string> StartAdvertisingAsync(string serviceName, string name, CancellationToken cancellationToken)
        {
            this.logger.Verbose($"{serviceName}, {name} ENTER");

            var result = await NearbyClass.Connections.StartAdvertisingAsync(Api, name, serviceName,
                new OnConnectionLifecycleCallback(
                    new NearbyConnectionLifeCycleCallback(OnInitiatedConnection, OnConnectionResult, OnDisconnected),
                    cancellationToken),
                new AdvertisingOptions(Strategy.P2pStar))
                .ConfigureAwait(false);

            if (!result.Status.IsSuccess)
            {
                var status = ToConnectionStatus(result.Status);
                throw new NearbyConnectionException("Failed to start advertising. " + status.StatusMessage, status.Status);
            }

            return result.LocalEndpointName;
        }

        public async Task<NearbyStatus> RequestConnectionAsync(string name, string endpoint, CancellationToken cancellationToken)
        {
            logger.Verbose($"({name}, {endpoint}) EXECUTE");
            var nearbyStatus = await ExecuteNearbyActionWithTimeoutAsync(
                NearbyClass.Connections.RequestConnection(Api, name, endpoint,
                    new OnConnectionLifecycleCallback(
                        new NearbyConnectionLifeCycleCallback(
                            OnInitiatedConnection,
                            OnConnectionResult,
                            OnDisconnected), cancellationToken)))
                .ConfigureAwait(false);

            LogNonSuccesfulResult(nearbyStatus, new ActionArgs(("name", name), ("endpoint", endpoint)));

            logger.Verbose($"({name}, {endpoint}) EXECUTE DONE => {nearbyStatus.Status.ToString()}");

            return ToConnectionStatus(nearbyStatus);
        }

        public async Task<NearbyStatus> AcceptConnectionAsync(string endpoint)
        {
            logger.Verbose($"({endpoint}) ENTER");
            
            var result = await ExecuteNearbyActionWithTimeoutAsync(
                NearbyClass.Connections.AcceptConnection(Api, endpoint,
                    new OnPayloadCallback(new NearbyPayloadCallback(OnPayloadReceived, OnPayloadTransferUpdate))))
                .ConfigureAwait(false);

            LogNonSuccesfulResult(result, new ActionArgs(("endpoint", endpoint)));

            logger.Verbose($"({endpoint}) EXIT => {result.Status}");

            return ToConnectionStatus(result);
        }

        public async Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload)
        {
            this.logger.Verbose($"({to}, '{payload}') ENTER");

            if (!(payload is Entities.Payload send))
                throw new ArgumentException($"Cannot handle payload of type: {payload.GetType().FullName}", nameof(payload));

            var result = await ExecuteNearbyActionWithTimeoutAsync(
                NearbyClass.Connections.SendPayload(Api, to, send.NearbyPayload))
                .ConfigureAwait(false);

            LogNonSuccesfulResult(result, new ActionArgs(("to", to)));

            this.logger.Verbose($"({to}, '{payload}') EXIT => {result.Status}");

            return ToConnectionStatus(result);
        }

        private static async Task<Statuses> ExecuteNearbyActionWithTimeoutAsync(PendingResult nearbyAction)
        {
            using (nearbyAction)
            using (var awaitableResultCallback = new AwaitableResultCallback<Statuses>())
            {
                nearbyAction.SetResultCallback(awaitableResultCallback, 30, TimeUnit.Seconds);

                var statuses = await awaitableResultCallback.AwaitAsync()
                    .ConfigureAwait(false);

                return statuses;
            }
        }

        public void StopAllEndpoint()
        {
            this.logger.Verbose();
            NearbyClass.Connections.StopAllEndpoints(Api);
        }

        public void StopDiscovery()
        {
            this.logger.Verbose();
            NearbyClass.Connections.StopDiscovery(Api);
        }

        public void StopAdvertising()
        {
            this.logger.Verbose();
            NearbyClass.Connections.StopAdvertising(Api);
        }

        public void StopAll()
        {
            this.logger.Verbose();
            this.StopAdvertising();
            this.StopDiscovery();
            this.StopAllEndpoint();
        }

        private void OnPayloadTransferUpdate(NearbyPayloadTransferUpdate payloadTransferUpdate)
        {
            PayloadTransferUpdate?.Invoke(this, payloadTransferUpdate);
        }

        private void OnPayloadReceived(IPayload payload)
        {
            PayloadReceived?.Invoke(this, payload);
        }

        private void OnDisconnected(string endpoint)
        {
            Disconnected?.Invoke(this, endpoint);
        }

        private void OnConnectionResult(NearbyConnectionResolution connectionResolution)
        {
            ConnectionResult?.Invoke(this, connectionResolution);
        }

        private void OnInitiatedConnection(NearbyConnectionInfo info)
        {
            InitiatedConnection?.Invoke(this, info);
        }

        private void LogNonSuccesfulResult(Statuses result, ActionArgs args, [CallerMemberName] string method = null)
        {
            if (result.IsSuccess) return;

            NearbyStatus nearbyStatus = ToConnectionStatus(result);
            if (nearbyStatus.Status == ConnectionStatusCode.StatusAlreadyConnectedToEndpoint)
                return;

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
                exception.Data[exceptionData.key] = exceptionData.data;

            logger.Error("Connection error", exception);
        }

        protected virtual void OnLostEndpoint(string endpoint)
        {
            LostEndpoint?.Invoke(this, endpoint);
        }

        protected virtual void OnFoundEndpoint(NearbyDiscoveredEndpointInfo info)
        {
            FoundEndpoint?.Invoke(this, info);
        }

        private class ActionArgs
        {
            public ActionArgs(params (string key, string data)[] data)
            {
                Data = data;
            }

            public (string key, string data)[] Data { get; }
        }

        private static NearbyStatus ToConnectionStatus(Statuses statuses)
        {
            var status = Enum.IsDefined(typeof(ConnectionStatusCode), statuses.StatusCode)
                ? (ConnectionStatusCode)statuses.StatusCode
                : ConnectionStatusCode.Unknown;

            return new NearbyStatus
            {
                IsSuccess = statuses.IsSuccess,
                IsCanceled = statuses.IsCanceled,
                StatusMessage = statuses.StatusMessage,
                IsInterrupted = statuses.IsInterrupted,
                StatusCode = statuses.StatusCode,
                Status = status
            };
        }
    }
}
