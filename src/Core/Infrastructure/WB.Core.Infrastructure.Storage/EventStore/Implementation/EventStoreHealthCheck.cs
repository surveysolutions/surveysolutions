using System;
using System.Net;
using EventStore.ClientAPI;
using WB.Core.Infrastructure.HealthCheck;

namespace WB.Core.Infrastructure.Storage.EventStore.Implementation
{
    public class EventStoreHealthCheck : IEventStoreHealthCheck
    {
        private readonly EventStoreConnectionSettings connectionSettings;

        public EventStoreHealthCheck(EventStoreConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        public ConnectionHealthCheckResult Check()
        {
            try
            {
                var settings = ConnectionSettings.Create();
                var serverIp = IPAddress.Parse(this.connectionSettings.ServerIP);
                var tcpEndPoint = new IPEndPoint(serverIp, this.connectionSettings.ServerTcpPort);

                var eventStoreConnection = EventStoreConnection.Create(settings, tcpEndPoint);
                eventStoreConnection.Close();

                return ConnectionHealthCheckResult.Happy();
            }
            catch (Exception e)
            {
                return ConnectionHealthCheckResult.Down(e.Message);
            }
        }
    }
}