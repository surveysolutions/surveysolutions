using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    public interface IAggregateRootCacheFiller
    {
        void Store(IEventSourcedAggregateRoot aggregateRoot);
    }
}