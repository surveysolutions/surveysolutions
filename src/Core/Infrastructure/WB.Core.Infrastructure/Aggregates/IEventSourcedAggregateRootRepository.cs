using System;
using System.Collections.Generic;
using System.Threading;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IEventSourcedAggregateRootRepository
    {
        IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId);
        IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<int> progress, CancellationToken cancellationToken);
    }
}