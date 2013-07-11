using System;

namespace Ncqrs.Eventing.Storage
{
    using System.Collections.Generic;

    public interface IStreamableEventStore : IEventStore
    {
        IEnumerable<CommittedEvent> GetEventStream();

        int CountOfAllEventsWithoutSnapshots();

        int CountOfAllEventsIncludingSnapshots();

        IEnumerable<CommittedEvent[]> GetAllEventsWithoutSnapshots(int bulkSize = 256);

        IEnumerable<CommittedEvent[]> GetAllEventsIncludingSnapshots(int bulkSize = 32);
    }
}
