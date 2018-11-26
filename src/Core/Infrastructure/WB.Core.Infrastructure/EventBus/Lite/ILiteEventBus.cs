using System.Collections.Generic;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.EventBus.Lite
{
    public delegate void EventsPublished(PublishedEventsArgs args);

    public class PublishedEventsArgs
    {
        public PublishedEventsArgs(IEnumerable<CommittedEvent> publishedEvents)
        {
            this.PublishedEvents = publishedEvents;
        }

        public IEnumerable<CommittedEvent> PublishedEvents { get; private set; }
    }

    public interface ILiteEventBus
    {
        IEnumerable<CommittedEvent> CommitUncommittedEvents(IEventSourcedAggregateRoot aggregateRoot, string origin);
        void PublishCommittedEvents(IEnumerable<CommittedEvent> committedEvents);
    }
}
