using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby.Connection;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.UI.Shared.Enumerator.OfflineSync.Entities;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation
{
    [Localizable(false)]
    public class NearbyConnectionClient : INearbyConnectionClient
    {
        private readonly IGoogleApiClientFactory apiClientFactory;
        private readonly ILogger logger;

        public NearbyConnectionClient(ILogger logger, IGoogleApiClientFactory apiClientFactory)
        {
            this.logger = logger;
            this.apiClientFactory = apiClientFactory;
        }

        private IConnectionsClient Api => apiClientFactory?.ConnectionsClient
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

            try
            {
                await Api.StartDiscoveryAsync(serviceName,
                    new OnDiscoveryCallback(OnFoundEndpoint, OnLostEndpoint, cancellationToken),
                    new DiscoveryOptions.Builder().SetStrategy(Strategy.P2pStar).Build());
            }
            catch(ApiException api)
            {
                return Error(api);
            }

            return NearbyStatus.Ok;
        }

        public async Task<string> StartAdvertisingAsync(string serviceName, string name, CancellationToken cancellationToken)
        {
            this.logger.Verbose($"{serviceName}, {name} ENTER");

            try
            {
                await Api.StartAdvertisingAsync(name, serviceName,
                new OnConnectionLifecycleCallback(
                    new NearbyConnectionLifeCycleCallback(OnInitiatedConnection, OnConnectionResult, OnDisconnected),
                    cancellationToken),
                 new AdvertisingOptions.Builder().SetStrategy(Strategy.P2pStar).Build())
                .ConfigureAwait(false);
            }
            catch (ApiException e)
            {
                return e.Message;
            }
            return name;
        }

        public async Task<NearbyStatus> RequestConnectionAsync(string name, string endpoint, CancellationToken cancellationToken)
        {
            logger.Verbose($"({name}, {endpoint}) EXECUTE");

            try
            {
                await Api.RequestConnectionAsync(name, endpoint,
                        new OnConnectionLifecycleCallback(
                            new NearbyConnectionLifeCycleCallback(
                                OnInitiatedConnection,
                                OnConnectionResult,
                                OnDisconnected), cancellationToken));

                return NearbyStatus.Ok;
            }
            catch (ApiException api)
            {
                logger.Error($"({endpoint})", api);
                return Error(api);
            }
        }

        public async Task<NearbyStatus> AcceptConnectionAsync(string endpoint)
        {
            logger.Verbose($"({endpoint}) ENTER");

            try
            {
                await Api.AcceptConnectionAsync(endpoint,
                        new OnPayloadCallback(new NearbyPayloadCallback(OnPayloadReceived, OnPayloadTransferUpdate)));

                return NearbyStatus.Ok;
            }
            catch(ApiException api)
            {
                logger.Error($"({endpoint})", api);
                return Error(api);
            }
        }

        private NearbyStatus Error(ApiException api)
        {
            return new NearbyStatus
            {
                IsSuccess = false,
                Status = (ConnectionStatusCode) api.StatusCode,
                StatusMessage = api.Message,
                StatusCode = api.StatusCode
            };
        }

        public async Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload)
        {
            // this.logger.Verbose($"({to}, '{payload}') ENTER");

            if (!(payload is Entities.Payload send))
                throw new ArgumentException($"Cannot handle payload of type: {payload.GetType().FullName}",
                    nameof(payload));

            try
            {
                await Api.SendPayloadAsync(to, send.NearbyPayload);

                return NearbyStatus.Ok;
            }
            catch (ApiException api)
            {
                return Error(api);
            }
        }

        public void StopAllEndpoint()
        {
            this.logger.Verbose();
            Api.StopAllEndpoints();
        }

        public void StopDiscovery()
        {
            this.logger.Verbose();
            Api.StopDiscovery();
        }

        public void StopAdvertising()
        {
            this.logger.Verbose();
            Api.StopAdvertising();
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
    }
}
