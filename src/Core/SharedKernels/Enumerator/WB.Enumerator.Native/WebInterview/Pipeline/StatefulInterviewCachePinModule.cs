using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public class StatefulInterviewCachePinModule : IPipelineModule
    {
        private readonly IAggregateRootCache aggregateRootCache;

        public StatefulInterviewCachePinModule(IAggregateRootCache aggregateRootCache)
        {
            this.aggregateRootCache = aggregateRootCache;
        }

        public Task OnConnected(Hub hub)
        {
            aggregateRootCache.PinItem(hub.GetInterviewId(), TimeSpan.MaxValue);
            return Task.CompletedTask;
        }

        public Task OnDisconnected(Hub hub, Exception exception)
        {
            aggregateRootCache.UnpinItem(hub.GetInterviewId());
            return Task.CompletedTask;
        }
    }
}
