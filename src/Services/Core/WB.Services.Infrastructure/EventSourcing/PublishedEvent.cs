using System;

namespace WB.Services.Infrastructure.EventSourcing
{
    public class PublishedEvent<T> where T : IEvent
    {
        public PublishedEvent(T @event, Guid eventSourceId, long globalSequence, long sequence)
        {
            Event = @event;
            EventSourceId = eventSourceId;
            GlobalSequence = globalSequence;
            Sequence = sequence;
        }

        public PublishedEvent(Event ev)
        {
            this.EventSourceId = ev.EventSourceId;
            this.Event = (T) ev.Payload;
            this.GlobalSequence = ev.GlobalSequence;
            this.Sequence = ev.Sequence;
        }

        public Guid EventSourceId { get; }
        public long GlobalSequence { get; }
        public long Sequence { get; }
        public T Event { get; }
    }
}
