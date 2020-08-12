using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public class StatefulInterviewCachePinModule : IPipelineModule
    {
        private readonly IAggregateRootCache aggregateRootCache;
        private readonly IAggregateLock aggregateLock;


        public StatefulInterviewCachePinModule(IAggregateRootCache aggregateRootCache, IAggregateLock aggregateLock)
        {
            this.aggregateRootCache = aggregateRootCache;
            this.aggregateLock = aggregateLock;
        }

        public Task OnConnected(Hub hub)
        {
            aggregateLock.RunWithLock(hub.GetInterviewIdString(), () =>
            {
                var aggregateId = hub.GetInterviewId();
                var count = this.aggregateRootCache.GetConnectedCount(aggregateId);
                this.aggregateRootCache.SetConnectedCount(aggregateId, ++count);

                aggregateRootCache.Update(hub.GetInterviewId(), TimeSpan.MaxValue);
            });
            
            return Task.CompletedTask;
        }

        public Task OnDisconnected(Hub hub, Exception exception)
        {
            aggregateLock.RunWithLock(hub.GetInterviewIdString(), () =>
            {
                var aggregateId = hub.GetInterviewId();
                var count = this.aggregateRootCache.GetConnectedCount(aggregateId);
                count--;
                this.aggregateRootCache.SetConnectedCount(aggregateId, count);
                
                if (count <= 0)
                {
                    aggregateRootCache.Update(hub.GetInterviewId());
                }
            });
            
            return Task.CompletedTask;
        }
    }
}
