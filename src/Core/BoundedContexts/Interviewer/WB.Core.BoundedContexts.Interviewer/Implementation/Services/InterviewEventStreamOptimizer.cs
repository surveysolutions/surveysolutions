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
            return interviewEvents.Where(storedEvent => !ShouldNotSendEvent(storedEvent)).ToReadOnlyCollection();
        }

        private static bool ShouldNotSendEvent(CommittedEvent storedEvent)
            => IsInterviewerOnly(storedEvent.Payload)
            || IsCalculatedButNotAggregating(storedEvent);

        private static bool IsInterviewerOnly(IEvent eventPayload)
            => eventPayload is InterviewAnswersFromSyncPackageRestored
            || eventPayload is InterviewOnClientCreated
            || eventPayload is InterviewSynchronized;

        private static bool IsCalculatedButNotAggregating(CommittedEvent storedEvent)
            => IsCalculated(storedEvent.Payload)
            && !IsAggregating(storedEvent);

        private static bool IsCalculated(IEvent eventPayload)
            => eventPayload is AnswersDeclaredValid
            || eventPayload is AnswersDeclaredInvalid
            || eventPayload is QuestionsEnabled
            || eventPayload is QuestionsDisabled
            || eventPayload is GroupsEnabled
            || eventPayload is GroupsDisabled;

        private static bool IsAggregating(CommittedEvent storedEvent)
            => false;
    }
}