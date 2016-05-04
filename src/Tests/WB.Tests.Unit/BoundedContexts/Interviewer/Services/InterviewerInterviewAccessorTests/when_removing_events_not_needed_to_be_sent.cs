using System;
using System.Collections.Generic;
using Machine.Specifications;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerInterviewAccessorTests
{
    internal class when_removing_events_not_needed_to_be_sent
    {
        Establish context = () =>
        {
            var firstCompletionCommitId = Guid.Parse("11111111111111111111111111111111");
            var lastCompletionCommitId = Guid.Parse("99999999999999999999999999999999");

            eventStream = new[]
            {
                questionAnswered = Create.CommittedEvent(payload: Create.Event.TextQuestionAnswered()),
                Create.CommittedEvent(payload: Create.Event.GroupsDisabled()),
                Create.CommittedEvent(payload: Create.Event.GroupsEnabled()),
                Create.CommittedEvent(payload: Create.Event.QuestionsDisabled()),
                Create.CommittedEvent(payload: Create.Event.QuestionsEnabled()),
                Create.CommittedEvent(payload: Create.Event.AnswersDeclaredInvalid()),
                Create.CommittedEvent(payload: Create.Event.AnswersDeclaredValid()),

                Create.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.GroupsDisabled()),
                Create.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.GroupsEnabled()),
                Create.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.QuestionsDisabled()),
                Create.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.QuestionsEnabled()),
                Create.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.AnswersDeclaredInvalid()),
                Create.CommittedEvent(commitId: firstCompletionCommitId, payload: Create.Event.AnswersDeclaredValid()),
                firstCompletion = Create.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.InteviewCompleted()),

                lastAggregatedGroupsDisabled = Create.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.GroupsDisabled()),
                lastAggregatedGroupsEnabled = Create.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.GroupsEnabled()),
                lastAggregatedQuestionsDisabled = Create.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.QuestionsDisabled()),
                lastAggregatedQuestionsEnabled = Create.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.QuestionsEnabled()),
                lastAggregatedQuestionsInvalid = Create.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.AnswersDeclaredInvalid()),
                lastAggregatedQuestionsValid = Create.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.AnswersDeclaredValid()),
                lastCompletion = Create.CommittedEvent(commitId: lastCompletionCommitId, payload: Create.Event.InteviewCompleted()),
            };

            optimizer = Create.InterviewEventStreamOptimizer();
        };

        Because of = () =>
            result = optimizer.RemoveEventsNotNeededToBeSent(eventStream);

        It should_remove_calculated_events_but_leave_calculated_events_from_last_interview_completion = () =>
            result.ShouldContainOnly(new[]
            {
                questionAnswered,

                firstCompletion,

                lastAggregatedGroupsDisabled,
                lastAggregatedGroupsEnabled,
                lastAggregatedQuestionsDisabled,
                lastAggregatedQuestionsEnabled,
                lastAggregatedQuestionsInvalid,
                lastAggregatedQuestionsValid,

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
        private static CommittedEvent lastCompletion;
    }
}