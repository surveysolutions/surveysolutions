using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IEventSourcedAggregateRoot : IAggregateRoot, IEventSource
    {
        bool HasUncommittedChanges();
        IEnumerable<UncommittedEvent> GetUnCommittedChanges();
        void MarkChangesAsCommitted();
    }
}