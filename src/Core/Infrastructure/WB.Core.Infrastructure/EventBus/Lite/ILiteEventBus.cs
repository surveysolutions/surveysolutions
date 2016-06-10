using System.Collections.Generic;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventBus
    {
        IEnumerable<CommittedEvent> CommitUncommittedEvents(IEventSourcedAggregateRoot aggregateRoot, string origin);
        void PublishCommittedEvents(IEnumerable<CommittedEvent> committedEvents);
    }
}