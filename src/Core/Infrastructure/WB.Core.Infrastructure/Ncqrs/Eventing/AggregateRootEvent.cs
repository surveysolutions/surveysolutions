using System;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace Main.Core.Events
{
    public class AggregateRootEvent
    {
        public AggregateRootEvent()
        {
        }

        public AggregateRootEvent(CommittedEvent committedEvent)
        {
            this.Payload = committedEvent.Payload;
            this.EventIdentifier = committedEvent.EventIdentifier;
            this.EventSequence = committedEvent.EventSequence;
            this.EventSourceId = committedEvent.EventSourceId;
            this.EventTimeStamp = committedEvent.EventTimeStamp;
            this.CommitId = committedEvent.CommitId;
            this.Origin = committedEvent.Origin;
        }

        public Guid CommitId { get; set; }

        public string Origin { get; set; }

        public Guid EventIdentifier { get; set; }

        public int EventSequence { get; set; }

        public Guid EventSourceId { get; set; }

        public DateTime EventTimeStamp { get; set; }

        public IEvent Payload { get; set; }
    }
}
