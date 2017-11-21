namespace WB.Core.Infrastructure.Aggregates
{
    public interface IEventSourcedAggregateRootRepositoryCacheCleaner
    {
        void CleanCache();
    }
}