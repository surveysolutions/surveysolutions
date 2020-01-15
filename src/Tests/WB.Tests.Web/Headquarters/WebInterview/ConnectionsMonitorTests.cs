using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Transports;
using Moq;
using Ncqrs;
using NUnit.Framework;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview
{
    [TestOf(typeof(ConnectionsMonitor))]
    public class ConnectionsMonitorTests
    {
        private SignalrResolverMock signalrResolver;

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            signalrResolver = new SignalrResolverMock();
        }

        [OneTimeTearDown]
        public void FixtureSetDown()
        {
            signalrResolver.Dispose();
        }

        [Test]
        public void should_drop_dead_and_not_alive_connections_during_check()
        {
            ITrackingConnection NewConnection(bool isAlive) => Mock.Of<ITrackingConnection>(tc =>
                tc.IsAlive == isAlive && tc.ConnectionId == Guid.NewGuid().ToString());
            
            var connections = new List<ITrackingConnection>
            {
                // live connection
                NewConnection(true),

                // marked as not alive
                NewConnection(false)
            };

            var zombie = NewConnection(true);

            var heartBeat = Mock.Of<ITransportHeartbeat>(th => th.GetConnections() == connections);
            var clock = new Mock<IClock>();

            signalrResolver.Mock<ITransportHeartbeat, ITransportHeartbeat>(heartBeat);

            var subj = new ConnectionsMonitor(clock.Object);

            var now = DateTime.UtcNow;

            // zombie connection - is a connection that were not tracked by disconnect event
            // it's last active time will not be updated by check function
            SetupClock(now.AddMinutes(-1));
            subj.Connected(zombie.ConnectionId); // register connection to connections list

            SetupClock(now);
            foreach (var connection in connections)
            {
                subj.Connected(connection.ConnectionId);
            }
            
            Assert.That(CommonMetrics.WebInterviewOpenConnections.Value, Is.EqualTo(3).Within(0.1));
            
            subj.Check();

            Assert.That(CommonMetrics.WebInterviewOpenConnections.Value, Is.EqualTo(1).Within(0.1));

            void SetupClock(DateTime value) => clock.Setup(c => c.UtcNow()).Returns(value);
        }
    }
}