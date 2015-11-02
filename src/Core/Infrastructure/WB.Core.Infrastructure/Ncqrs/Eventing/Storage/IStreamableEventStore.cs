using System;
using System.Collections.Generic;

namespace Ncqrs.Eventing.Storage
{
    public interface IStreamableEventStore : IEventStore
    {
        int CountOfAllEvents();

        IEnumerable<CommittedEvent> GetAllEvents();

        IEnumerable<EventSlice> GetEventsAfterPosition(EventPosition? position);
        long GetEventsCountAfterPosition(EventPosition? position);
    }
}
