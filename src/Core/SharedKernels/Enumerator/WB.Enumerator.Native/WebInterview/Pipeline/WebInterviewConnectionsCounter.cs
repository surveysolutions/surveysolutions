using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    [Localizable(false)]
    public class WebInterviewConnectionsCounter : IPipelineModule
    {
        private readonly IConnectionsMonitor connectionsMonitor;
        
        public WebInterviewConnectionsCounter(IConnectionsMonitor connectionsMonitor)
        {
            this.connectionsMonitor = connectionsMonitor;
        }
        
        public Task OnConnected(Hub hub)
        {
            connectionsMonitor.Connected(hub.Context.ConnectionId);
            return Task.CompletedTask;
        }

        public Task OnDisconnected(Hub hub, Exception exception)
        {
            connectionsMonitor.Disconnected(hub.Context.ConnectionId);
            return Task.CompletedTask;
        }

        /*public Task OnReconnected(Hub hub)
        {
            connectionsMonitor.Connected(hub.Context.ConnectionId);
            return Task.CompletedTask;
        }*/
    }
}
