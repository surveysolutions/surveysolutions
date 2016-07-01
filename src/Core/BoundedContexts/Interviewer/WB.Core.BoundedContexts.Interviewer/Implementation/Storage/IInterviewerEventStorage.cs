using System;
using Ncqrs.Eventing.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Storage
{
    public interface IInterviewerEventStorage : IEventStore
    {
        void RemoveEventSourceById(Guid interviewId);
        int GetMaxEventSequenceById(Guid interviewId);
    }
}