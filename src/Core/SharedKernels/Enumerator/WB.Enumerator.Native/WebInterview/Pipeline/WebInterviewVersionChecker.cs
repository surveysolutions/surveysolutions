using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public class WebInterviewVersionChecker : IPipelineModule
    {
        private readonly IProductVersion productVersion;

        public WebInterviewVersionChecker(IProductVersion productVersion)
        {
            this.productVersion = productVersion;
        }

        private Task ReloadIfOlderVersion(Hub hub)
        {
            var clientVersion = hub.Context.Items[@"appVersion"] as string;
            if (this.productVersion.ToString() != clientVersion)
            {
                return hub.Clients.Caller.SendCoreAsync("reloadInterview", null, CancellationToken.None);
            }

            return Task.CompletedTask;
        }

        public Task OnConnected(Hub hub)
        {
            return this.ReloadIfOlderVersion(hub);
        }

        public Task OnDisconnected(Hub hub, Exception exception)
        {
            return Task.CompletedTask;
        }

        /*public Task OnReconnected(Hub hub)
        {
            this.ReloadIfOlderVersion(hub);
            return Task.CompletedTask;
        }*/
    }
}
