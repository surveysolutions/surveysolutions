using System;

namespace Ncqrs.Eventing.Storage
{
    public struct EventPosition
    {
        public EventPosition(long commitPosition, long preparePosition, Guid eventSourceIdOfLastEvent,
            int sequenceOfLastEvent)
        {
            this.CommitPosition = commitPosition;
            this.PreparePosition = preparePosition;
            this.EventSourceIdOfLastEvent = eventSourceIdOfLastEvent;
            this.SequenceOfLastEvent = sequenceOfLastEvent;
        }

        public long CommitPosition { get; private set; }
        public long PreparePosition { get; private set; }

        public Guid EventSourceIdOfLastEvent { get; private set; }
        public int SequenceOfLastEvent { get; private set; }
    }
}