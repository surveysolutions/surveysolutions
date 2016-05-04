using System.Collections.Generic;
using Ncqrs.Eventing;

namespace WB.Core.BoundedContexts.Interviewer.Services.Infrastructure
{
    public interface IInterviewEventStreamOptimizer
    {
        IReadOnlyCollection<CommittedEvent> RemoveEventsNotNeededToBeSent(IReadOnlyCollection<CommittedEvent> interviewEvents);
    }
}