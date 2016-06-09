using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Storage
{
    public interface IInterviewerEventStorage : IEventStore
    {
        void RemoveEventSourceById(Guid interviewId);
    }
}