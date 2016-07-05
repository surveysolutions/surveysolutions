using System;
using System.Collections.Generic;
using System.Threading;
using Ncqrs.Eventing.Storage;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IEventSourcedAggregateRootRepository
    {
        IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId);
        IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken);
        IEventSourcedAggregateRoot GetStateless(Type aggregateType, Guid aggregateId);
    }
}