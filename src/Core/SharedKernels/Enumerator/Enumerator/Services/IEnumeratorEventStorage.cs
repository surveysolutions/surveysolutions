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
        List<CommittedEvent> GetPendingEvents(Guid interviewId);
        bool HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(long sequence, Guid eventSourceId, params string[] typeNames);
    }
}
