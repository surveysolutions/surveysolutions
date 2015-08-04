using WB.Core.Infrastructure.Aggregates;


namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventBus
    {
        void CommitUncommittedEvents(IAggregateRoot aggregateRoot, string origin);
        void PublishUncommittedEvents(IAggregateRoot aggregateRoot, bool isBulk = false);
    }
}