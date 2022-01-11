using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class InterviewEventStreamOptimizer : IInterviewEventStreamOptimizer
    {
        public IReadOnlyCollection<CommittedEvent> FilterEventsToBeSent(
            IEnumerable<CommittedEvent> interviewEvents, Guid? lastCompletionCommitId)
        {
            if (!lastCompletionCommitId.HasValue)
                return interviewEvents.ToReadOnlyCollection();

            return interviewEvents.Where(@event => !ShouldNotSendEvent(@event, lastCompletionCommitId.Value)).ToReadOnlyCollection();
        }
        
        private static bool ShouldNotSendEvent(CommittedEvent committedEvent, Guid lastCompletionCommitId)
            => IsInterviewerOnly(committedEvent.Payload)
            || IsCalculatedButNotAggregating(committedEvent, lastCompletionCommitId);

        private static bool IsInterviewerOnly(IEvent eventPayload)
            => eventPayload is InterviewSynchronized;

        private static bool IsCalculatedButNotAggregating(CommittedEvent committedEvent, Guid lastCompletionCommitId)
            => IsCalculated(committedEvent.Payload)
            && !IsFromLastCompletion(committedEvent, lastCompletionCommitId);

        private static bool IsCalculated(IEvent eventPayload)
            => eventPayload is AnswersDeclaredValid
            || eventPayload is AnswersDeclaredInvalid
            || eventPayload is QuestionsEnabled
            || eventPayload is QuestionsDisabled
            || eventPayload is GroupsEnabled
            || eventPayload is GroupsDisabled
            || eventPayload is StaticTextsDisabled
            || eventPayload is StaticTextsEnabled
            || eventPayload is StaticTextsDeclaredValid
            || eventPayload is StaticTextsDeclaredInvalid
            || eventPayload is LinkedOptionsChanged
            || eventPayload is LinkedToListOptionsChanged
            || eventPayload is VariablesDisabled
            || eventPayload is VariablesEnabled 
            || eventPayload is VariablesChanged
            || eventPayload is SubstitutionTitlesChanged;

        private static bool IsFromLastCompletion(CommittedEvent committedEvent, Guid lastCompletionCommitId)
            => committedEvent.CommitId == lastCompletionCommitId;
    }
}
