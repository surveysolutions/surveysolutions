using Ncqrs.Domain;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Eventing.Sourcing.Snapshotting
{
    /// <summary>
    /// A snapshotting policy which disables snapshotting for all aggregates.
    /// </summary>
    public class NoSnapshottingPolicy : ISnapshottingPolicy
    {
        public bool ShouldCreateSnapshot(IEventSourcedAggregateRoot aggregateRoot)
        {
            return false;
        }
    }
}