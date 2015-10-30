using System;
using System.Collections;
using System.Collections.Generic;

namespace Ncqrs.Eventing.Storage
{
    public interface IStreamableEventStore : IEventStore
    {
        int CountOfAllEvents();

        IEnumerable<CommittedEvent> GetAllEvents();

        IEnumerable<EventSlice> GetEventsAfterPosition(EventPosition? position);
    }

    public class EventSlice : IEnumerable<CommittedEvent>
    {
        private readonly IEnumerable<CommittedEvent> events;

        public EventSlice(IEnumerable<CommittedEvent> events, EventPosition position)
        {
            this.events = events;
            this.Position = position;
        }

        public EventPosition Position { get; private set; }
        public IEnumerator<CommittedEvent> GetEnumerator()
        {
            return events.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
