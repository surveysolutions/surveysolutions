using System;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.Infrastructure.EventBus.Lite.Implementation.RaiseFilters
{
    public class AggregateRootRaiseFilter : ILiteEventRaiseFilter
    {
        private readonly Guid aggregateRootId;

        public AggregateRootRaiseFilter(string aggregateRootId)
        {
            if (aggregateRootId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(aggregateRootId));

            this.aggregateRootId = Guid.Parse(aggregateRootId); 
        }

        public bool IsNeedRaise(CommittedEvent @event)
        {
            return @event.EventSourceId == this.aggregateRootId;
        }
    }
}