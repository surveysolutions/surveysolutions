﻿using System;

namespace WB.Services.Infrastructure.EventSourcing
{
    public class PublishedEvent<T> where T : IEvent
    {
        public PublishedEvent(T @event, Guid eventSourceId, long globalSequence, long sequence, DateTime eventTimeStamp)
        {
            Event = @event;
            EventSourceId = eventSourceId;
            GlobalSequence = globalSequence;
            Sequence = sequence;
            EventTimeStamp = eventTimeStamp;
        }

        public PublishedEvent(Event ev)
        {
            this.EventSourceId = ev.EventSourceId;
            this.Event = (T) ev.Payload;
            this.GlobalSequence = ev.GlobalSequence;
            this.Sequence = ev.Sequence;
            this.EventTimeStamp = ev.EventTimeStamp;
        }

        public Guid EventSourceId { get; }
        public long GlobalSequence { get; }
        public long Sequence { get; }
        public DateTime EventTimeStamp { get; }
        public T Event { get; }
    }
}
