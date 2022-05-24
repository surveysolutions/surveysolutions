using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IEnumeratorEventStorage : IEventStore
    {
        void RemoveEventSourceById(Guid interviewId);
        void StoreEvents(CommittedEventStream events);
        int GetLastEventKnownToHq(Guid interviewId);
        
        bool HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(long sequence, Guid eventSourceId, params string[] typeNames);
        CommittedEvent GetEventByEventSequence(Guid eventSourceId, int eventSequence);

        int GetMaxSequenceForAnyEvent(Guid interviewId, params string[] typeNames);

        List<Guid> GetListOfAllItemsIds();
        void MarkAllEventsAsReceivedByHq(Guid interviewId);

        void InsertEventsFromHqInEventsStream(Guid interviewId, CommittedEventStream events);
        bool IsLastEventInSequence(Guid interviewId, Guid eventId);
        Guid? GetLastEventIdUploadedToHq(Guid interviewId);
        Guid? GetFirstEventId(Guid interviewId);
        bool HasEventsWithoutHqFlag(Guid interviewId);
    }
}
