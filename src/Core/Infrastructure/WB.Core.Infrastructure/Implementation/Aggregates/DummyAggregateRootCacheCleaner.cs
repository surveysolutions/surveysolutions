using System;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    public class DummyAggregateRootCacheCleaner : IAggregateRootCacheCleaner
    {
        public void Evict(Guid aggregateId)
        {
        }
    }
}
