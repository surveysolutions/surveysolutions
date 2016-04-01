using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IAggregateRoot : IEventSource
    {
        void SetId(Guid id);
        bool HasUncommittedChanges();
        IEnumerable<UncommittedEvent> GetUnCommittedChanges();
        void MarkChangesAsCommitted();
    }
}