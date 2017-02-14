using System;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    public interface IAggregateRootCacheCleaner
    {
        void Evict(Guid aggregateId);
    }
}