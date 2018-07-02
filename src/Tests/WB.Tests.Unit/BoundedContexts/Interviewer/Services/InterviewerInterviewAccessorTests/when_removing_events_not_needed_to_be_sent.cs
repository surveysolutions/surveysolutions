using System;
using System.Collections.Generic;
using FluentAssertions;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerInterviewAccessorTests
{
    internal class when_removing_events_not_needed_to_be_sent
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var firstCompletionCommitId = Guid.Parse("11111111111111111111111111111111");
            var lastCompletionCommitId = Guid.Parse("99999999999999999999999999999999");

            eventStream = new[]
            {
                questionAnswered = Create.Other.CommittedEvent(payload: Create.Event.TextQuestionAnswered()),
                Create.Other.CommittedEvent(payload: Create.Event.GroupsDisabled()),
                Create.Other.CommittedEvent(payload: Create.Event.GroupsEnabled()),
                Create.Other.CommittedEvent(payload: Create.Event.QuestionsDisabled()),
                Create.Other.CommittedEvent(payload: Create.Event.QuestionsEnabled()),
                Create.Other.CommittedEvent(payload: Create.Event.AnswersDeclaredInvalid()),
                Create.Other.CommittedEvent(payload: Create.Event.AnswersDeclaredValid()),
                Create.Other.CommittedEvent(payload: Create.Event.StaticTextsDisabled()),
                Create.Other.CommittedEvent(payload: Create.Event.StaticTextsEnabled()),
                Create.Other.CommittedEvent(payload: Create.Event.StaticTextsDeclaredInvalid()),
                Create.Other.CommittedEvent(payload: Create.Event.StaticTextsDeclaredValid()),
                Create.Other.CommittedEvent(payload: Create.Event.LinkedOptionsChanged()),
                Create.Other.CommittedEvent(payload: Create.Event.VariablesDisabled()),
                Create.Other.CommittedEvent(payload: Create.Event.VariablesEnabled()),
                Create.Other.CommittedEvent(payload: Create.Event.VariablesChanged()),
                Create.Other.CommittedEvent(payload: Create.Event.SubstitutionTitlesChanged()),

                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.GroupsDisabled()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.GroupsEnabled()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.QuestionsDisabled()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.QuestionsEnabled()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.AnswersDeclaredInvalid()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.AnswersDeclaredValid()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.StaticTextsDisabled()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.StaticTextsEnabled()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.StaticTextsDeclaredInvalid()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.StaticTextsDeclaredValid()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.LinkedOptionsChanged()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.VariablesDisabled()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.VariablesEnabled()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.VariablesChanged()),
                Create.Other.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.SubstitutionTitlesChanged()),
                firstCompletion = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.InteviewCompleted()),

                lastAggregatedGroupsDisabled = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.GroupsDisabled()),
                lastAggregatedGroupsEnabled = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.GroupsEnabled()),
                lastAggregatedQuestionsDisabled = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.QuestionsDisabled()),
                lastAggregatedQuestionsEnabled = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.QuestionsEnabled()),
                lastAggregatedQuestionsInvalid = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.AnswersDeclaredInvalid()),
                lastAggregatedQuestionsValid = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.AnswersDeclaredValid()),
                lastAggregatedStaticTextsValid = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.StaticTextsDeclaredValid()),
                lastAggregatedStaticTextsInvalid = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.StaticTextsDeclaredInvalid()),
                lastAggregatedStaticTextsEnabled = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.StaticTextsEnabled()),
                lastAggregatedStaticTextsDisabled = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.StaticTextsDisabled()),
                lastAggregatedLinkedOptionsChanged = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.LinkedOptionsChanged()),
                lastAggregatedVariablesDisabled = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.VariablesDisabled()),
                lastAggregatedVariablesEnabled = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.VariablesEnabled()),
                lastAggregatedVariablesValuesChanged = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.VariablesChanged()),
                lastAggregatedSubstitutionTitlesChanged = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.SubstitutionTitlesChanged()),
                lastCompletion = Create.Other.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.InteviewCompleted()),
            };

            optimizer = Create.Service.InterviewEventStreamOptimizer();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = optimizer.RemoveEventsNotNeededToBeSent(eventStream);

        [NUnit.Framework.Test] public void should_remove_calculated_events_but_leave_calculated_events_from_last_interview_completion () =>
            result.Should().BeEquivalentTo(new[]
            {
                questionAnswered,

                firstCompletion,

                lastAggregatedGroupsDisabled,
                lastAggregatedGroupsEnabled,
                lastAggregatedQuestionsDisabled,
                lastAggregatedQuestionsEnabled,
                lastAggregatedQuestionsInvalid,
                lastAggregatedQuestionsValid,
                lastAggregatedStaticTextsValid,
                lastAggregatedStaticTextsInvalid,
                lastAggregatedStaticTextsEnabled,
                lastAggregatedStaticTextsDisabled,
                lastAggregatedLinkedOptionsChanged,
                lastAggregatedVariablesDisabled,
                lastAggregatedVariablesEnabled,
                lastAggregatedVariablesValuesChanged,
                lastAggregatedSubstitutionTitlesChanged,

                lastCompletion,
            });

        private static InterviewEventStreamOptimizer optimizer;
        private static IReadOnlyCollection<CommittedEvent> eventStream;
        private static IReadOnlyCollection<CommittedEvent> result;
        private static CommittedEvent questionAnswered;
        private static CommittedEvent firstCompletion;
        private static CommittedEvent lastAggregatedGroupsDisabled;
        private static CommittedEvent lastAggregatedGroupsEnabled;
        private static CommittedEvent lastAggregatedQuestionsDisabled;
        private static CommittedEvent lastAggregatedQuestionsEnabled;
        private static CommittedEvent lastAggregatedQuestionsInvalid;
        private static CommittedEvent lastAggregatedQuestionsValid;
        private static CommittedEvent lastAggregatedStaticTextsValid;
        private static CommittedEvent lastAggregatedStaticTextsInvalid;
        private static CommittedEvent lastAggregatedStaticTextsEnabled;
        private static CommittedEvent lastAggregatedStaticTextsDisabled;
        private static CommittedEvent lastAggregatedLinkedOptionsChanged;
        private static CommittedEvent lastAggregatedVariablesDisabled;
        private static CommittedEvent lastAggregatedVariablesEnabled;
        private static CommittedEvent lastAggregatedVariablesValuesChanged;
        private static CommittedEvent lastCompletion;
        private static CommittedEvent lastAggregatedSubstitutionTitlesChanged;
    }
}
