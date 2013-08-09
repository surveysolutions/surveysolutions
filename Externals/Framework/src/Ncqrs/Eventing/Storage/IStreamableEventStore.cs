using System;

namespace Ncqrs.Eventing.Storage
{
    using System.Collections.Generic;

    public interface IStreamableEventStore : IEventStore
    {
        IEnumerable<CommittedEvent> GetEventStream();

        int CountOfAllEvents();

        IEnumerable<CommittedEvent[]> GetAllEvents(int bulkSize = 32);
    }
}
