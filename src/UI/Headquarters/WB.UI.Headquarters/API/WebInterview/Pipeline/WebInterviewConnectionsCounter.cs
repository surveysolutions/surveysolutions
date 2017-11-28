using System.ComponentModel;
using Microsoft.AspNet.SignalR.Hubs;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    [Localizable(false)]
    public class WebInterviewConnectionsCounter : HubPipelineModule
    {
        private readonly IConnectionsMonitor connectionsMonitor;
        
        public WebInterviewConnectionsCounter(IConnectionsMonitor connectionsMonitor)
        {
            this.connectionsMonitor = connectionsMonitor;
        }
        
        protected override void OnAfterConnect(IHub hub)
        {
            connectionsMonitor.Connected(hub.Context.ConnectionId);
            base.OnAfterConnect(hub);
        }

        protected override void OnAfterReconnect(IHub hub)
        {
            connectionsMonitor.Connected(hub.Context.ConnectionId);
            base.OnAfterReconnect(hub);
        }

        protected override bool OnBeforeDisconnect(IHub hub, bool stopCalled)
        {
            connectionsMonitor.Disconnected(hub.Context.ConnectionId);
            return base.OnBeforeDisconnect(hub, stopCalled);
        }
    }
}