using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation
{
    [ExcludeFromCodeCoverage]
    public class NearbyConnection : INearbyConnection, IDisposable
    {
        private readonly INearbyCommunicator communicator;
        private readonly ILogger logger;
        private readonly INearbyConnectionClient connectionClient;
        private readonly Subject<INearbyEvent> events;

        public NearbyConnection(INearbyCommunicator communicator, ILogger logger, INearbyConnectionClient connectionClient)
        {
            this.communicator = communicator;
            this.logger = logger;
            this.connectionClient = connectionClient;
            this.connectionClient.LostEndpoint += LostEndpoint;
            this.connectionClient.FoundEndpoint += FoundEndpoint;
            this.connectionClient.ConnectionResult += OnConnectionClientResult;
            this.connectionClient.Disconnected += OnDisconnected;
            this.connectionClient.InitiatedConnection += OnInitiatedConnectionClient;
            this.connectionClient.PayloadReceived += OnPayloadReceived;
            this.connectionClient.PayloadTransferUpdate += OnPayloadTransferUpdate;

            events = new Subject<INearbyEvent>();
            Events = events;
            Task.Factory.StartNew(CommunicationQueue, TaskCreationOptions.LongRunning);
        }

        public async Task<NearbyStatus> StartDiscoveryAsync(string serviceName, CancellationToken cancellationToken)
        {
            var result = await connectionClient.StartDiscoveryAsync(serviceName, cancellationToken);

            this.logger.Info($"[START DISCOVERY] of {serviceName}");
            this.logger.Info($"[START DISCOVERY] Result - {ToString(result)}");

            return result;
        }

        private string ToString(NearbyStatus result)
        {
            return $"Status: {result.Status} Code: {result.StatusCode} Message: {result.StatusMessage}";
        }

        public void StopAllEndpoint()
        {
            this.connectionClient.StopAllEndpoint();
            this.logger.Info($"Stop all Nearby Endpoints");
        }

        public async Task<string> StartAdvertisingAsync(string serviceName, string name, CancellationToken cancellationToken)
        {
            this.connectionClient.StopAdvertising();
            //this.logger.Verbose($"{serviceName}, {name}) ENTER");
            var result = await this.connectionClient.StartAdvertisingAsync(serviceName, name, cancellationToken);
            this.logger.Info($"[START ADVERTISING] of {serviceName} by {name}");
            this.logger.Info($"[START ADVERTISING] Result {result}");
            return result;
        }

        private readonly ConcurrentDictionary<string, bool> pendingRequestConnections =
            new ConcurrentDictionary<string, bool>();

        public async Task<NearbyStatus> RequestConnectionAsync(string name, string endpoint,
            CancellationToken cancellationToken)
        {
            this.logger.Verbose($"[REQUEST CONNECTION]: ({name}, {endpoint}) ENTER");

            // if there is already successful pending request. Than do nothing
            if (pendingRequestConnections.TryGetValue(endpoint, out _))
            {
                this.logger.Verbose($"[REQUEST CONNECTION]: ({name}, {endpoint}) EXIT. There is already pending requests");

                return new NearbyStatus
                {
                    IsSuccess = true,
                    Status = ConnectionStatusCode.StatusAlreadyConnectedToEndpoint
                };
            }

            NearbyStatus result;
            try
            {
                this.StopDiscovery();
                result = await this.connectionClient.RequestConnectionAsync(name, endpoint, cancellationToken);
            }
            catch (Exception e)
            {
                result = new NearbyStatus
                {
                    IsCanceled = true,
                    IsSuccess = false,
                    Status = ConnectionStatusCode.StatusError,
                    StatusMessage = e.Message
                };
            }

            if (result.Status == ConnectionStatusCode.StatusAlreadyConnectedToEndpoint)
            {
                this.OnConnectionClientResult(endpoint, new NearbyConnectionResolution { IsSuccess = true });
            }

            if (result.IsSuccess)
            {
                this.logger.Verbose($"[REQUEST CONNECTION]: ({name}, {endpoint}) Added pending request connection.");
                pendingRequestConnections.TryAdd(endpoint, true);
            }

            this.logger.Verbose($"[REQUEST CONNECTION]: ({name}, {endpoint}) => RequestConnectionAsync ended {result.Status.ToString()}");
            return result;
        }

        public async Task<NearbyStatus> AcceptConnectionAsync(string endpoint)
        {
            var status = await this.connectionClient.AcceptConnectionAsync(endpoint);

            this.logger.Info($"[ACCEPT CONNECTION] to endpoint: {endpoint}. Status: {ToString(status)}");
            return status;
        }

        public async Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload)
        {
            var status = await this.connectionClient.SendPayloadAsync(to, payload);
            return status;
        }

        public IObservable<INearbyEvent> Events { get; }

        public ObservableCollection<RemoteEndpoint> RemoteEndpoints { get; } =
            new ObservableCollection<RemoteEndpoint>();

        public void StopDiscovery()
        {
            try
            {

                this.logger.Info($"[STOP DISCOVERY]");
                this.connectionClient.StopDiscovery();
            }
            catch (Exception e)
            {
                this.logger.Error("[STOP DISCOVERY] Failed", e);
            }
        }

        public void StopAdvertising()
        {
            try
            {
                this.connectionClient.StopAdvertising();
                this.logger.Info($"[STOP ADVERTISING]");
            }
            catch (Exception e)
            {
                this.logger.Error("[STOP ADVERTISING] Failed", e);
            }
        }

        public void StopAll()
        {
            this.logger.Verbose();
            try
            {
                this.connectionClient.StopAll();
                this.pendingRequestConnections.Clear();

                this.logger.Info("[STOP ALL]");
            }
            catch (Exception e)
            {
                this.logger.Error("[STOP ALL] Failed", e);
                /* om om om */
            }
        }

        private void LostEndpoint(object sender, string endpoint)
        {
            this.logger.Verbose($"[LOST ENDPOINT] ({endpoint}) ENTER");

            if (knownEndpoints.TryRemove(endpoint, out var name))
            {
                this.logger.Verbose($"({endpoint}) Remove known: '{name ?? "<unknown>"}'. Notify.");
                events.OnNext(new NearbyEvent.EndpointLost(endpoint));
            }
        }

        private void FoundEndpoint(object sender, NearbyDiscoveredEndpointInfo info)
        {
            this.logger.Verbose($"[FOUND ENDPOINT] ({info.Endpoint}, {info.EndpointName}) ENTER");

            this.logger.Verbose($"({info.Endpoint}) Add known: '{info.EndpointName ?? "<unknown>"}'. Notify.");
            events.OnNext(new NearbyEvent.EndpointFound(info.Endpoint, info));
        }

        private void OnPayloadTransferUpdate(object sender, NearbyPayloadTransferUpdate update)
        {
            // this.logger.Verbose($"({update.Endpoint}, payloadId: {update.Id}, status: {update.Status.ToString()}, {update.BytesTransferred} of {update.TotalBytes}");
            this.communicationQueue.Post(update);
        }

        protected virtual void OnDisconnected(object sender, string endpoint)
        {
            this.logger.Verbose($"[DISCONNECTED] ({endpoint}) ENTER");
            var existing = this.RemoteEndpoints.FirstOrDefault(re => re.Endpoint == endpoint);
            events.OnNext(new NearbyEvent.Disconnected(endpoint, existing?.Name));

            if (existing != null)
            {
                this.RemoteEndpoints.Remove(existing);
            }
        }

        private void OnConnectionClientResult(object sender, NearbyConnectionResolution resolution)
        {
            this.logger.Verbose($"[OnConnectionClientResult] ({resolution.Endpoint}, {resolution.IsSuccess}) ENTER. Remove pending request");

            if (resolution.IsSuccess && !string.IsNullOrWhiteSpace(resolution.Endpoint))
            {
                var endpoint = resolution.Endpoint;
                pendingRequestConnections.TryRemove(endpoint, out _);

                var existing = this.RemoteEndpoints.FirstOrDefault(re => re.Endpoint == endpoint);

                if (existing != null)
                {
                    this.RemoteEndpoints.Remove(existing);
                }

                knownEndpoints.TryGetValue(endpoint, out var name);

                this.logger.Verbose($"[OnConnectionClientResult] Connected to endpoint: {endpoint}. Name: {name}");
                this.RemoteEndpoints.Add(new RemoteEndpoint { Endpoint = endpoint, Name = name });
                events.OnNext(new NearbyEvent.Connected(endpoint, name));
            }
        }

        private readonly ConcurrentDictionary<string, string> knownEndpoints = new ConcurrentDictionary<string, string>();

        private void OnInitiatedConnectionClient(object sender, NearbyConnectionInfo info)
        {
            this.logger.Verbose($"[OnInitiatedConnectionClient] ({info.Endpoint}, name: {info.EndpointName}," +
                                $" incoming: {info.IsIncomingConnection}, auth: {info.AuthenticationToken})");
            knownEndpoints.TryAdd(info.Endpoint, info.EndpointName);
            events.OnNext(new NearbyEvent.InitiatedConnection(info.Endpoint, info));
        }

        public void Dispose()
        {
            events?.Dispose();

            this.connectionClient.LostEndpoint -= LostEndpoint;
            this.connectionClient.FoundEndpoint -= FoundEndpoint;

            this.connectionClient.ConnectionResult -= OnConnectionClientResult;
            this.connectionClient.Disconnected -= OnDisconnected;
            this.connectionClient.InitiatedConnection -= OnInitiatedConnectionClient;

            this.connectionClient.PayloadReceived -= OnPayloadReceived;
            this.connectionClient.PayloadTransferUpdate -= OnPayloadTransferUpdate;
        }

        private BufferBlock<object> communicationQueue = new BufferBlock<object>(new DataflowBlockOptions
        {
            EnsureOrdered = true
        });

        private async Task CommunicationQueue()
        {
            while (await communicationQueue.OutputAvailableAsync())
            {
                var queue = await communicationQueue.ReceiveAsync();

                if (queue is IPayload payload)
                {
                    await communicator.ReceivePayloadAsync(this, payload.Endpoint, payload);
                }

                if (queue is NearbyPayloadTransferUpdate update)
                {
                    communicator.ReceivePayloadTransferUpdate(this, update.Endpoint, update);
                }
            }
        }

        private void OnPayloadReceived(object sender, IPayload payload)
        {
            // this.logger.Verbose($"GOT ==> ({payload.Endpoint}, {payload})");
            communicationQueue.Post(payload);
        }
    }
}
