using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.EventBus
{
    // TODO: TLK, KP-4337: include it's functionality to event bus when it will be made portable
    public interface IEventPublisher
    {
        void PublishUncommitedEventsFromAggregateRoot(IAggregateRoot aggregateRoot, string origin);
    }
}