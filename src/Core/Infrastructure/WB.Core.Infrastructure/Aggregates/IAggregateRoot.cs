namespace WB.Core.Infrastructure.Aggregates
{
    public interface IAggregateRoot
    {
        void MarkChangesAsCommitted();
    }
}