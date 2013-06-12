using System;

namespace Ncqrs.Eventing.Storage
{
    using System.Collections.Generic;

    public interface IStreamableEventStore : IEventStore
    {
        IEnumerable<CommittedEvent> GetEventStream();

        #warning move to readlayer
        Guid? GetLastEvent(Guid aggregateRootId);

        #warning move to readlayer
        bool IsEventPresent(Guid aggregateRootId, Guid eventIdentifier);

        #warning move to readlayer
        CommittedEventStream ReadFromWithoutPayload(Guid id, long minVersion, long maxVersion);

        int CountOfAllEventsWithoutSnapshots();

        int CountOfAllEventsIncludingSnapshots();

        IEnumerable<CommittedEvent[]> GetAllEventsWithoutSnapshots(int bulkSize = 256);

        IEnumerable<CommittedEvent[]> GetAllEventsIncludingSnapshots(int bulkSize = 32);
    }
}
