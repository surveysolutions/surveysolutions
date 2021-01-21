using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    [Localizable(false)]
    public class WebInterviewConnectionsCounter : IPipelineModule
    {
        private readonly IServiceLocator serviceLocator;

        public WebInterviewConnectionsCounter(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public Task OnConnected(Hub hub)
        {
            var mode = GetMode(hub);
            CommonMetrics.WebInterviewConnection.Labels("open", mode, workspaceContext).Inc();
            return Task.CompletedTask;
        }

        public Task OnDisconnected(Hub hub, Exception exception)
        {
            var mode = GetMode(hub);
            CommonMetrics.WebInterviewConnection.Labels("closed", mode, workspaceContext).Inc();
            return Task.CompletedTask;
        }

        private string workspaceContext => serviceLocator.CurrentWorkspace()?.Name ?? string.Empty;
        private string GetMode(Hub hub) => hub.Context.GetHttpContext().Request.Query["mode"];
    }
}
