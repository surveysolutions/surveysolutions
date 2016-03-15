using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Events;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewEventStreamOptimizer : IInterviewEventStreamOptimizer
    {
        public IReadOnlyCollection<CommittedEvent> RemoveEventsNotNeededToBeSent(
            IReadOnlyCollection<CommittedEvent> interviewEvents)
        {
            return interviewEvents.Where(@event => !ShouldNotSendEvent(@event)).ToReadOnlyCollection();
        }

        private static bool ShouldNotSendEvent(CommittedEvent committedEvent)
            => IsInterviewerOnly(committedEvent.Payload)
            || IsCalculatedButNotAggregating(committedEvent);

        private static bool IsInterviewerOnly(IEvent eventPayload)
            => eventPayload is InterviewAnswersFromSyncPackageRestored
            || eventPayload is InterviewOnClientCreated
            || eventPayload is InterviewSynchronized;

        private static bool IsCalculatedButNotAggregating(CommittedEvent committedEvent)
            => IsCalculated(committedEvent.Payload)
            && !IsAggregating(committedEvent);

        private static bool IsCalculated(IEvent eventPayload)
            => eventPayload is AnswersDeclaredValid
            || eventPayload is AnswersDeclaredInvalid
            || eventPayload is QuestionsEnabled
            || eventPayload is QuestionsDisabled
            || eventPayload is GroupsEnabled
            || eventPayload is GroupsDisabled;

        private static bool IsAggregating(CommittedEvent committedEvent)
            => false;
    }
}