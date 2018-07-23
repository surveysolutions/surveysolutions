using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
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

        private NamedAsyncLocker locker = new NamedAsyncLocker();

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
        }

        public async Task<NearbyStatus> StartDiscoveryAsync(string serviceName, CancellationToken cancellationToken)
        {
            //this.logger.Verbose($"({serviceName}) ENTER");

            var result = await connectionClient.StartDiscoveryAsync(serviceName, cancellationToken);

            //this.logger.Verbose($"({serviceName}) EXIT => {result.Status.ToString()}");
            return result;
        }

        public void StopAllEndpoint()
        {
            this.connectionClient.StopAllEndpoint();
        }

        public async Task<string> StartAdvertisingAsync(string serviceName, string name, CancellationToken cancellationToken)
        {
            //this.logger.Verbose($"{serviceName}, {name}) ENTER");
            var result = await this.connectionClient.StartAdvertisingAsync(serviceName, name, cancellationToken);
            return result;
        }

        private readonly ConcurrentDictionary<string, bool> pendingRequestConnections =
            new ConcurrentDictionary<string, bool>();

        public async Task<NearbyStatus> RequestConnectionAsync(string name, string endpoint,
            CancellationToken cancellationToken)
        {
            this.logger.Verbose($"({name}, {endpoint}) ENTER. Waiting lock");

            cancellationToken.ThrowIfCancellationRequested();

            return await locker.RunWithLockAsync(endpoint, async () =>
            {
                this.logger.Verbose($"({name}, {endpoint}) ENTER. Done waiting lock");

                // if there is already succesfull pending request. Than do nothing
                if (pendingRequestConnections.TryGetValue(endpoint, out _))
                {
                    this.logger.Verbose($"({name}, {endpoint}) EXIT. There is already pending requests");

                    return new NearbyStatus
                    {
                        IsSuccess = true,
                        Status = ConnectionStatusCode.StatusOk
                    };
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

                NearbyStatus result;
                try
                {
                   result = await this.connectionClient.RequestConnectionAsync(name, endpoint, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    result = new NearbyStatus
                    {
                        IsCanceled = true,
                        IsSuccess = false,
                        Status = ConnectionStatusCode.StatusError
                    };
                }

                if (result.Status == ConnectionStatusCode.StatusAlreadyConnectedToEndpoint)
                {
                    this.OnConnectionClientResult(endpoint, new NearbyConnectionResolution { IsSuccess = true });
                }

                if (result.IsSuccess)
                {
                    this.logger.Verbose($"({name}, {endpoint}) Added pending request connection.");
                    pendingRequestConnections.TryAdd(endpoint, true);
                }

                this.logger.Verbose($"({name}, {endpoint}) => RequestConnectionAsync ended {result.Status.ToString()}");
                return result;
            }).ConfigureAwait(false);
        }

        public Task<NearbyStatus> AcceptConnectionAsync(string endpoint, CancellationToken cancellationToken)
        {
            return this.connectionClient.AcceptConnectionAsync(endpoint, cancellationToken);
        }

        public Task<NearbyStatus> RejectConnectionAsync(string endpoint, CancellationToken cancellationToken)
        {
            return this.connectionClient.RejectConnectionAsync(endpoint, cancellationToken);
        }

        public async Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload,
            CancellationToken cancellationToken)
        {
            var result = await this.connectionClient.SendPayloadAsync(to, payload, cancellationToken);
            return result;
        }

        public IObservable<INearbyEvent> Events { get; }

        public ObservableCollection<RemoteEndpoint> RemoteEndpoints { get; } =
            new ObservableCollection<RemoteEndpoint>();

        public void StopDiscovery()
        {
            try
            {
                this.connectionClient.StopDiscovery();
            } catch { }
        }

        public void StopAdvertising()
        {
            try
            {
                this.connectionClient.StopAdvertising();
            } catch { }
        }

        public void StopAll()
        {
            this.logger.Verbose();
            try
            {
                this.connectionClient.StopAll();
                this.RemoteEndpoints.Clear();
                this.knownEnpoints.Clear();
                this.pendingRequestConnections.Clear();
                this.locker = new NamedAsyncLocker();
            }
            catch
            {
                /* om om om */
            }
        }

        private void LostEndpoint(object sender, string endpoint)
        {
            this.logger.Verbose($"({endpoint}) ENTER");

            if (knownEnpoints.TryRemove(endpoint, out var name))
            {
                this.logger.Verbose($"({endpoint}) Remove known: '{name ?? "<unknown>"}'. Notify.");
                events.OnNext(new NearbyEvent.EndpointLost(endpoint));
            }
        }

        private void FoundEndpoint(object sender, NearbyDiscoveredEndpointInfo info)
        {
            this.logger.Verbose($"({info.Endpoint}, {info.EndpointName}) ENTER");

            if (knownEnpoints.TryAdd(info.Endpoint, info.EndpointName))
            {
                this.logger.Verbose($"({info.Endpoint}) Add known: '{info.EndpointName ?? "<unknown>"}'. Notify.");
                events.OnNext(new NearbyEvent.EndpointFound(info.Endpoint, info));
            }
        }

        protected virtual void OnDisconnected(object sender, string endpoint)
        {
            this.logger.Verbose($"({endpoint}) ENTER");

            var exising = this.RemoteEndpoints.FirstOrDefault(re => re.Enpoint == endpoint);
            events.OnNext(new NearbyEvent.Disconnected(endpoint, exising?.Name));

            if (exising != null)
            {
                this.RemoteEndpoints.Remove(exising);
            }
        }

        private void OnConnectionClientResult(object sender, NearbyConnectionResolution resolution)
        {
            var endpoint = resolution.Endpoint;
            this.logger.Verbose($"({endpoint}, {resolution.IsSuccess}) ENTER. Remove pending request");
            pendingRequestConnections.TryRemove(endpoint, out _);

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

        private readonly ConcurrentDictionary<string, string>
            knownEnpoints = new ConcurrentDictionary<string, string>();

        private void OnInitiatedConnectionClient(object sender, NearbyConnectionInfo info)
        {
            this.logger.Verbose(
                $"({info.Endpoint}, name: {info.EndpointName}, incoming: {info.IsIncomingConnection}, auth: {info.AuthenticationToken})");
            knownEnpoints.TryAdd(info.Endpoint, info.EndpointName);
            events.OnNext(new NearbyEvent.InitiatedConnection(info.Endpoint, info));
        }

        private void OnPayloadTransferUpdate(object sender, NearbyPayloadTransferUpdate update)
        {
            this.logger.Verbose($"({update.Endpoint}, payloadId: {update.Id}, status: {update.Status.ToString()}, {update.BytesTransferred} of {update.TotalBytes}");
            communicator.RecievePayloadTransferUpdate(this, update.Endpoint, update);
        }

        private async void OnPayloadReceived(object sender, IPayload payload)
        {
            this.logger.Verbose($"({payload.Endpoint}, {payload.ToString()})");
            await communicator.RecievePayloadAsync(this, payload.Endpoint, payload);
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
    }
}
