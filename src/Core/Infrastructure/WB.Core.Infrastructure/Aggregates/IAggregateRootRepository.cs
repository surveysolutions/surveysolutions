using System;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IAggregateRootRepository
    {
        IAggregateRoot GetLatest(Type aggregateType, Guid aggregateId);
    }
}