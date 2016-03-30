using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;


namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventBus
    {
        CommittedEventStream CommitUncommittedEvents(IEventSourcedAggregateRoot aggregateRoot, string origin);
        void PublishCommittedEvents(CommittedEventStream committedEvents);
    }
}