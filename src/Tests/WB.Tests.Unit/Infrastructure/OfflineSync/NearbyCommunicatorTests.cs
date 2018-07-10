using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
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

        [Test]
        public async Task simple_communication_send_recieve_protocol_test()
        {
            // setup server
            var serverhandler = Create.Service.GoogleConnectionsRequestHandler()
                .WithSampleEchoHandler();

            var server = Create.Service.NearbyConnectionManager(serverhandler);

            // client
            var client = Create.Service.NearbyConnectionManager();

            var clientCommunicator = Create.Fake.GoogleConnection()
                .WithTwoWayClientServerConnectionMap(server, client);

            // act
            var id = Guid.NewGuid();

            var response = await client.SendAsync<PingMessage, PongMessage>(clientCommunicator, "server", 
                new PingMessage { Id = id }, null, CancellationToken.None);

            Assert.That(response.Id, Is.EqualTo(id), "Ensure that we indeed handle proper request");
        }

        [TestCase(5, 0 ,0, Description = "Should timeout before SUP even recieve package")]
        [TestCase(0, 5, 0, Description = "Should timeout after SUP recieve package")]
        [TestCase(0, 0, 5, Description = "Should timeout before success packages received by SUP")]
        public void should_throw_on_connection_timeout_at_certain_delays(params int[] delaysInSeconds)
        {
            // setting up timeout ot one second
            NearbyCommunicator.MessageAwaitingTimeout = TimeSpan.FromSeconds(1);

            // setup server
            var serverhandler = Create.Service.GoogleConnectionsRequestHandler()
                .WithSampleEchoHandler();

            var server = Create.Service.NearbyConnectionManager(serverhandler);

            // client
            var client = Create.Service.NearbyConnectionManager();
            var clientCommunicator = Create.Fake.GoogleConnection()
                .WithTwoWayClientServerConnectionMap(server, client)
                .WithDelaysOnResponse(TimeSpan.FromSeconds(1));

            // act

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await client.SendAsync<PingMessage, PongMessage>(clientCommunicator, "server", 
                    new PingMessage(), null, CancellationToken.None);
            });
        }
    }
}
