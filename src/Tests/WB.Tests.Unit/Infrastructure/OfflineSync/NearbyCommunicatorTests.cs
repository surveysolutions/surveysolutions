using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;

namespace WB.Tests.Unit.Infrastructure.OfflineSync
{
    [TestFixture]
    public class NearbyCommunicatorTests
    {
        [SetUp]
        public void Setup()
        {
            if (Debugger.IsAttached) // UPDATE TIMEOUT IF DEBUGGING
            {
                NearbyCommunicator.MessageAwaitingTimeout = TimeSpan.FromMinutes(10);
            }
        }

        [TestCase(0, 4, Description = "Limit on max bytes should be not enought to fit in header. Should execute 4 sendings")]
        [TestCase(10000000, 2, Description = "Limit on max bytes should be enough to fit in header. Should execute 2 sendings ")]
        public async Task simple_communication_send_recieve_protocol_test(int maxBytes, long expectedRequestsCount)
        {
            using (new CommunicationSession())
            {
                // setup server
                var serverhandler = Create.Service.GoogleConnectionsRequestHandler()
                    .WithSampleEchoHandler();

                var server = Create.Service.NearbyConnectionManager(serverhandler, maxBytesLength: maxBytes);

                // client
                var client = Create.Service.NearbyConnectionManager(maxBytesLength: maxBytes);

                var clientCommunicator = Create.Fake.GoogleConnection()
                    .WithTwoWayClientServerConnectionMap(server, client);

                // act
                var id = Guid.NewGuid();

                var response = await client.SendAsync<PingMessage, PongMessage>(clientCommunicator, "server",
                    new PingMessage { Id = id }, null, CancellationToken.None);

                Assert.That(response.Id, Is.EqualTo(id), "Ensure that we indeed handle proper request");
                Assert.That(CommunicationSession.Current.RequestsTotal, Is.EqualTo(expectedRequestsCount));
            }
        }


        //[TestCase(5, 0, 0, Description = "Should timeout before SV even recieve package")]
        //[TestCase(0, 5, 0, Description = "Should timeout after SV recieve package")]
        //[TestCase(0, 0, 5, Description = "Should timeout before success packages received by SV")]
        //public void should_throw_on_connection_timeout_at_certain_delays(params int[] delaysInSeconds)
        //{
        //    // setting up timeout ot one second
        //    NearbyCommunicator.MessageAwaitingTimeout = TimeSpan.FromSeconds(1);

        //    // setup server
        //    var serverhandler = Create.Service.GoogleConnectionsRequestHandler()
        //        .WithSampleEchoHandler();

        //    var server = Create.Service.NearbyConnectionManager(serverhandler);

        //    // client
        //    var client = Create.Service.NearbyConnectionManager();
        //    var clientCommunicator = Create.Fake.GoogleConnection()
        //        .WithTwoWayClientServerConnectionMap(server, client)
        //        .WithDelaysOnResponse(TimeSpan.FromSeconds(1));

        //    // act

        //    Assert.ThrowsAsync<TaskCanceledException>(async () =>
        //    {
        //        await client.SendAsync<PingMessage, PongMessage>(clientCommunicator, "server",
        //            new PingMessage(), null, CancellationToken.None);
        //    });
        //}

        [Test]
        public void should_throw_communication_exception_if_failed_response_recieved()
        {
            // setup server
            var serverhandler = Create.Service.GoogleConnectionsRequestHandler()
                .WithHandler<PingMessage, PongMessage>(ping => throw new Exception());

            var server = Create.Service.NearbyConnectionManager(serverhandler);

            // client
            var client = Create.Service.NearbyConnectionManager();

            var clientCommunicator = Create.Fake.GoogleConnection()
                .WithTwoWayClientServerConnectionMap(server, client);

            // act
            var id = Guid.NewGuid();

            Assert.ThrowsAsync<CommunicationException>(async () => await client.SendAsync<PingMessage, PongMessage>(clientCommunicator, "server",
                new PingMessage { Id = id }, null, CancellationToken.None));

        }

        [Test]
        public async Task should_process_stream_payload_when_success_update_arrives_before_payload_registration()
        {
            using (new CommunicationSession())
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
            {
                var serverhandler = Create.Service.GoogleConnectionsRequestHandler()
                    .WithSampleEchoHandler();

                var server = Create.Service.NearbyConnectionManager(serverhandler, maxBytesLength: 0);
                var client = Create.Service.NearbyConnectionManager(maxBytesLength: 0);

                var clientCommunicator = new OutOfOrderPayloadTransferConnection((from, _, payload) =>
                    payload.Type == PayloadType.Bytes ? (TransferStatus?)null : TransferStatus.Success)
                    .WithTwoWayClientServerConnectionMap(server, client);

                var id = Guid.NewGuid();

                var response = await client.SendAsync<PingMessage, PongMessage>(clientCommunicator, "server",
                    new PingMessage { Id = id }, null, cts.Token);

                Assert.That(response.Id, Is.EqualTo(id));
            }
        }

        [Test]
        public void should_cancel_pending_response_when_failure_update_arrives_before_payload_registration()
        {
            using (new CommunicationSession())
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                var serverhandler = Create.Service.GoogleConnectionsRequestHandler()
                    .WithSampleEchoHandler();

                var server = Create.Service.NearbyConnectionManager(serverhandler, maxBytesLength: 0);
                var client = Create.Service.NearbyConnectionManager(maxBytesLength: 0);

                var clientCommunicator = new OutOfOrderPayloadTransferConnection((from, _, payload) =>
                    payload.Type != PayloadType.Bytes && from == "server"
                        ? TransferStatus.Failure
                        : (TransferStatus?)null)
                    .WithTwoWayClientServerConnectionMap(server, client);

                var id = Guid.NewGuid();

                Assert.CatchAsync<OperationCanceledException>(async () => await client.SendAsync<PingMessage, PongMessage>(
                    clientCommunicator, "server", new PingMessage { Id = id }, null, cts.Token));
                Assert.That(cts.IsCancellationRequested, Is.False);
            }
        }

        private sealed class OutOfOrderPayloadTransferConnection : INearbyConnection
        {
            private readonly IDictionary<string, INearbyCommunicator> clientsMap = new Dictionary<string, INearbyCommunicator>();
            private readonly IDictionary<string, string> connectionMap = new Dictionary<string, string>();
            private readonly Func<string, string, IPayload, TransferStatus?> receiverTerminalStatusSelector;

            public OutOfOrderPayloadTransferConnection(Func<string, string, IPayload, TransferStatus?> receiverTerminalStatusSelector)
            {
                this.receiverTerminalStatusSelector = receiverTerminalStatusSelector;
            }

            public OutOfOrderPayloadTransferConnection WithTwoWayClientServerConnectionMap(INearbyCommunicator server,
                INearbyCommunicator client)
            {
                return SetConnectionManager("server", server)
                    .SetConnectionManager("client", client)
                    .MapConnection("server", "client")
                    .MapConnection("client", "server");
            }

            public OutOfOrderPayloadTransferConnection SetConnectionManager(string endpoint, INearbyCommunicator manager)
            {
                clientsMap.Add(endpoint, manager);
                return this;
            }

            public OutOfOrderPayloadTransferConnection MapConnection(string fromEndpoint, string toEndpoint)
            {
                connectionMap.Add(fromEndpoint, toEndpoint);
                return this;
            }

            public async Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload)
            {
                var from = connectionMap[to];
                var toClient = clientsMap[to];
                var fromClient = clientsMap[from];
                var payloadForReceiver = payload.Type == PayloadType.Bytes
                    ? payload
                    : new PayloadThatMustBeReadBeforeReceiveReturns(payload);
                var receiverTerminalStatus = receiverTerminalStatusSelector(from, to, payload);

                if (payload.Type != PayloadType.Bytes)
                {
                    fromClient.ReceivePayloadTransferUpdate(this, to, new NearbyPayloadTransferUpdate
                    {
                        Status = TransferStatus.InProgress,
                        BytesTransferred = 0,
                        Id = payload.Id
                    });
                }

                fromClient.ReceivePayloadTransferUpdate(this, to, new NearbyPayloadTransferUpdate
                {
                    Status = TransferStatus.Success,
                    BytesTransferred = 100,
                    Id = payload.Id
                });

                if (payload.Type != PayloadType.Bytes && receiverTerminalStatus.HasValue)
                {
                    toClient.ReceivePayloadTransferUpdate(this, from, new NearbyPayloadTransferUpdate
                    {
                        Status = receiverTerminalStatus.Value,
                        BytesTransferred = 100,
                        Id = payload.Id
                    });
                }

                await toClient.ReceivePayloadAsync(this, from, payloadForReceiver);

                if (payload.Type != PayloadType.Bytes && !receiverTerminalStatus.HasValue)
                {
                    toClient.ReceivePayloadTransferUpdate(this, from, new NearbyPayloadTransferUpdate
                    {
                        Status = TransferStatus.Success,
                        BytesTransferred = 100,
                        Id = payload.Id
                    });
                }

                if (payloadForReceiver is PayloadThatMustBeReadBeforeReceiveReturns guardedPayload)
                {
                    guardedPayload.MarkReceiveCompleted();
                }

                return NearbyStatus.Ok;
            }

            public Task<NearbyStatus> StartDiscoveryAsync(string serviceName, CancellationToken cancellationToken)
                => throw new NotImplementedException();

            public Task<string> StartAdvertisingAsync(string serviceName, string name, CancellationToken cancellationToken)
                => throw new NotImplementedException();

            public Task<NearbyStatus> RequestConnectionAsync(string name, string endpoint, CancellationToken cancellationToken)
                => throw new NotImplementedException();

            public Task<NearbyStatus> AcceptConnectionAsync(string endpoint)
                => throw new NotImplementedException();

            public void StopAllEndpoint()
                => throw new NotImplementedException();

            public IObservable<INearbyEvent> Events { get; } = new Subject<INearbyEvent>();
            public ObservableCollection<RemoteEndpoint> RemoteEndpoints { get; } = new ObservableCollection<RemoteEndpoint>();

            public void StopDiscovery()
                => throw new NotImplementedException();

            public void StopAdvertising()
                => throw new NotImplementedException();

            public void StopAll()
                => throw new NotImplementedException();
        }

        private sealed class PayloadThatMustBeReadBeforeReceiveReturns : IPayload
        {
            private readonly byte[] streamBytes;
            private bool canRead = true;

            public PayloadThatMustBeReadBeforeReceiveReturns(IPayload payload)
            {
                Endpoint = payload.Endpoint;
                Bytes = payload.Bytes;
                Id = payload.Id;
                Type = payload.Type;
                streamBytes = ((MemoryStream)payload.Stream).ToArray();
                Stream = new MemoryStream(streamBytes);
            }

            public string Endpoint { get; }
            public byte[] Bytes { get; }
            public long Id { get; }
            public Stream Stream { get; }
            public PayloadType Type { get; }
            public byte[] BytesFromStream { get; private set; }

            public void MarkReceiveCompleted() => canRead = false;

            public void ReadStream()
            {
                if (!canRead)
                    throw new InvalidOperationException("The stream must be read before ReceivePayloadAsync returns.");

                BytesFromStream = streamBytes;
            }
        }
    }
}
