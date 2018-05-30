using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Storage
{
    public interface IInterviewerEventStorage : IEventStore
    {
        void RemoveEventSourceById(Guid interviewId);
        void StoreEvents(CommittedEventStream events);
        List<CommittedEvent> GetPendingEvents(Guid interviewId);
    }
}
