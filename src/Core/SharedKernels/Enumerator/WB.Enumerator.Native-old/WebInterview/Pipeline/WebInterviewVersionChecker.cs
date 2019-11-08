using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
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

        private void ReloadIfOlderVersion(IHub hub)
        {
            if (this.productVersion.ToString() != hub.Context.QueryString[@"appVersion"])
            {
                hub.Clients.Caller.reloadInterview();
            }
        }

        public Task OnConnected(IHub hub)
        {
            this.ReloadIfOlderVersion(hub);
            return Task.CompletedTask;
        }

        public Task OnDisconnected(IHub hub, bool stopCalled)
        {
            return Task.CompletedTask;
        }

        public Task OnReconnected(IHub hub)
        {
            this.ReloadIfOlderVersion(hub);
            return Task.CompletedTask;
        }
    }
}
