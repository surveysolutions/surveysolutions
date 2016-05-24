using Ncqrs.Eventing;

namespace WB.Core.Infrastructure.EventBus.Lite.Implementation.RaiseFilters
{
    public class AggregateRootRaiseFilter : ILiteEventRaiseFilter
    {
        private readonly string aggregateRootId;

        public AggregateRootRaiseFilter(string aggregateRootId)
        {
            this.aggregateRootId = aggregateRootId;
        }

        public bool IsNeedRaise(CommittedEvent @event)
        {
            return @event.EventSourceId.ToString() == this.aggregateRootId;
        }
    }
}