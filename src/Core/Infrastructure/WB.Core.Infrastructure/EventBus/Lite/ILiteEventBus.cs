using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;


namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventBus
    {
        CommittedEventStream CommitUncommittedEvents(IAggregateRoot aggregateRoot, string origin);
        void PublishCommitedEvents(CommittedEventStream committedEvents);
    }
}