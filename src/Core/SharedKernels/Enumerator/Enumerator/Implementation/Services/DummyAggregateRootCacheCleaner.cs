using System;
using WB.Core.Infrastructure.Implementation.Aggregates;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class DummyAggregateRootCacheCleaner : IAggregateRootCacheCleaner
    {
        public void Evict(Guid aggregateId)
        {
        }
    }
}
