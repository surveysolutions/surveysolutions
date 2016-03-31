using System;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.WriteSide
{
    public interface IEventSourcedAggregateRootRepositoryWithCache : IEventSourcedAggregateRootRepository
    {
        void CleanCache();
    }
}