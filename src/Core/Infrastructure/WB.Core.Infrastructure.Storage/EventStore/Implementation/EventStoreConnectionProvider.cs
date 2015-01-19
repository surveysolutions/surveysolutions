using System;
using System.Net;
using EventStore.ClientAPI;

namespace WB.Core.Infrastructure.Storage.EventStore.Implementation
{
    public class EventStoreConnectionProvider : IEventStoreConnectionProvider
    {
        private readonly EventStoreConnectionSettings connectionSettings;

        public EventStoreConnectionProvider(EventStoreConnectionSettings connectionSettings)
        {
            if (connectionSettings == null) throw new ArgumentNullException("connectionSettings");

            this.connectionSettings = connectionSettings;
        }

        public IEventStoreConnection Open()
        {
            var serverIp = IPAddress.Parse(this.connectionSettings.ServerIP);
            var tcpEndPoint = new IPEndPoint(serverIp, this.connectionSettings.ServerTcpPort);
            var eventStoreConnection = EventStoreConnection.Create(tcpEndPoint);

            return eventStoreConnection;
        }
    }
}