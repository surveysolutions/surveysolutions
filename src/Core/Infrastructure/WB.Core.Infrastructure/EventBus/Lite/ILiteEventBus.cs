using WB.Core.Infrastructure.Aggregates;


namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventBus
    {
        void PublishUncommitedEventsFromAggregateRoot(IAggregateRoot aggregateRoot, string origin);
    }
}