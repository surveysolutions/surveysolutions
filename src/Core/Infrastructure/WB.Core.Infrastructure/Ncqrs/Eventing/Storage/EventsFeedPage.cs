using System.Collections.Generic;

namespace Ncqrs.Eventing.Storage
{
    public class EventsFeedPage
    {
        public EventsFeedPage(long currentGlobalSequenceValue, IList<CommittedEvent> events)
        {
            CurrentGlobalSequenceValue = currentGlobalSequenceValue;
            Events = events;
        }

        public long CurrentGlobalSequenceValue { get; }

        public IList<CommittedEvent> Events { get; }
    }
}
