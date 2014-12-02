using System;
using System.Collections.Generic;
using Ncqrs.Eventing;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IAggregateRoot
    {
        void SetId(Guid id);
        IEnumerable<UncommittedEvent> GetUncommittedChanges();
        void MarkChangesAsCommitted();
    }
}