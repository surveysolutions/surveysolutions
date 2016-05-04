namespace WB.Core.Infrastructure.CommandBus.Implementation
{
    internal enum AggregateKind
    {
        EventSourced = 1,
        Plain = 2,
    }
}