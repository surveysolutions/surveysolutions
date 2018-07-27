using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
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
                    new PingMessage {Id = id}, null, CancellationToken.None);

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
    }
}
