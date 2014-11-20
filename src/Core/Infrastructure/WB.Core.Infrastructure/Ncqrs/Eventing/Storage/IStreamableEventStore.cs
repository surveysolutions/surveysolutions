using System;
using System.Collections.Generic;

namespace Ncqrs.Eventing.Storage
{
    public interface IStreamableEventStore : IEventStore
    {
        IEnumerable<CommittedEvent> GetEventStream();

        int CountOfAllEvents();

        IEnumerable<CommittedEvent[]> GetAllEvents(int bulkSize = 32, int skipEvents = 0);
        long GetLastEventSequence(Guid id);
    }
}
