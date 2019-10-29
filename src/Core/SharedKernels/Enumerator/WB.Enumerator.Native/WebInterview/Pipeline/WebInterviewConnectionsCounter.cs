using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;

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
        
        public Task OnConnected(IHub hub)
        {
            connectionsMonitor.Connected(hub.Context.ConnectionId);
            return Task.CompletedTask;
        }

        public Task OnDisconnected(IHub hub, bool stopCalled)
        {
            connectionsMonitor.Disconnected(hub.Context.ConnectionId);
            return Task.CompletedTask;
        }

        public Task OnReconnected(IHub hub)
        {
            connectionsMonitor.Connected(hub.Context.ConnectionId);
            return Task.CompletedTask;
        }
    }
}
