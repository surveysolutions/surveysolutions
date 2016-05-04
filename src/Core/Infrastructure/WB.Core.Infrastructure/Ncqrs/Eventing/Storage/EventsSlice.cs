using System.Collections;
using System.Collections.Generic;

namespace Ncqrs.Eventing.Storage
{
    public class EventSlice : IEnumerable<CommittedEvent>
    {
        private readonly IEnumerable<CommittedEvent> events;

        public EventSlice(IEnumerable<CommittedEvent> events, EventPosition position, bool isEndOfStream)
        {
            this.events = events;
            this.Position = position;
            this.IsEndOfStream = isEndOfStream;
        }

        public EventPosition Position { get; private set; }
        public bool IsEndOfStream { get; private set; }
        public IEnumerator<CommittedEvent> GetEnumerator()
        {
            return this.events.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}