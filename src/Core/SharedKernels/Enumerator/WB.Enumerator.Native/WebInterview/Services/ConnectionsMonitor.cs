using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Ncqrs;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class ConnectionsMonitor : IConnectionsMonitor
    {
        //private readonly ITransportHeartbeat transportHeartbeat;
        private readonly IClock clock;
        private readonly HubConnectionStore hubConnectionStore;

        private readonly ConcurrentDictionary<string, DateTime> connectedClients
            = new ConcurrentDictionary<string, DateTime>();

        private CancellationTokenSource cancellation;

        // How often we plan to check if the connections in our store are valid
        private readonly TimeSpan presenceCheckInterval = TimeSpan.FromSeconds(10);

        // The number of seconds that have to pass to consider a connection invalid.
        private readonly TimeSpan zombieThreshold = TimeSpan.FromSeconds(30);

        public ConnectionsMonitor(IClock clock, HubConnectionStore hubConnectionStore)
        {
            // ITransportHeartbeat is registered by SignalR, and accessible only via GlobalHost resolver
            //this.transportHeartbeat = GlobalHost.DependencyResolver.Resolve<ITransportHeartbeat>();
            this.clock = clock;
            this.hubConnectionStore = hubConnectionStore;
            CommonMetrics.WebInterviewOpenConnections.Set(0);
        }

        public void Connected(string connectionId)
        {
            connectedClients.AddOrUpdate(connectionId, clock.UtcNow(), (key, old) => clock.UtcNow());
            CommonMetrics.WebInterviewOpenConnections.Set(connectedClients.Count);
        }

        public void Disconnected(string connectionId)
        {
            connectedClients.TryRemove(connectionId, out _);
            CommonMetrics.WebInterviewOpenConnections.Set(connectedClients.Count);
        }

        public void StartMonitoring()
        {
            cancellation = new CancellationTokenSource();
            
            Task.Factory.StartNew(async () =>
            {
                Thread.CurrentThread.Name = "WebInterview Connections monitor thread";
                while (true)
                {
                    if (cancellation.Token.IsCancellationRequested) break;

                    Check();

                    await Task.Delay(presenceCheckInterval);
                }
            }, cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        // based on https://stackoverflow.com/a/21070978/41483
        public void Check()
        {
            foreach (var connection in hubConnectionStore)
            {
                if (connection.ConnectionAborted.IsCancellationRequested)
                {
                    Disconnected(connection.ConnectionId);
                    continue;
                }

                connectedClients.AddOrUpdate(connection.ConnectionId, clock.UtcNow(), (key, old) => clock.UtcNow());
            }

            var deadList = new List<string>();

            foreach (var connection in connectedClients)
            {
                if (clock.UtcNow() - connection.Value > zombieThreshold)
                {
                    deadList.Add(connection.Key);
                }
            }

            foreach (var dead in deadList)
            {
                Disconnected(dead);
            }
        }
    }
}
