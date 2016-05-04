using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IEventSourcedAggregateRootRepository
    {
        IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId);
    }
}