using System;

namespace Ncqrs.Eventing.Storage
{
    using System.Collections.Generic;

    public interface IStreamableEventStore : IEventStore
    {
        IEnumerable<CommittedEvent> GetEventStream();

        int CountOfAllEventsWithoutSnapshots();

        int CountOfAllEventsIncludingSnapshots();

        IEnumerable<CommittedEvent[]> GetAllEventsIncludingSnapshots(int bulkSize = 32);
        
    }
}
