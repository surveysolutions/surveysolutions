using System;
using System.Net;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

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
            var settings = ConnectionSettings
                .Create()
                .KeepReconnecting()
                .SetDefaultUserCredentials(new UserCredentials(this.connectionSettings.Login, this.connectionSettings.Password));

            var serverIp = IPAddress.Parse(this.connectionSettings.ServerIP);
            var tcpEndPoint = new IPEndPoint(serverIp, this.connectionSettings.ServerTcpPort);

            var eventStoreConnection = EventStoreConnection.Create(settings, tcpEndPoint);

            return eventStoreConnection;
        }
    }
}