using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    [Localizable(false)]
    public class WebInterviewConnectionsCounter : IPipelineModule
    {
        public Task OnConnected(Hub hub)
        {
            var mode = GetMode(hub);
            CommonMetrics.WebInterviewConnection.Labels("open", mode).Inc();
            return Task.CompletedTask;
        }

        public Task OnDisconnected(Hub hub, Exception exception)
        {
            var mode = GetMode(hub);
            CommonMetrics.WebInterviewConnection.Labels("closed", mode).Inc();
            return Task.CompletedTask;
        }

        private string GetMode(Hub hub) => hub.Context.GetHttpContext().Request.Query["mode"];
    }
}
