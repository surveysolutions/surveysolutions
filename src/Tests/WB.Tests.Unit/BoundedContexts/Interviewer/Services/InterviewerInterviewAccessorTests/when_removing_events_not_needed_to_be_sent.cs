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
            eventStream = new[]
            {
                questionAnswered = Create.CommittedEvent(payload: Create.Event.TextQuestionAnswered()),
                Create.CommittedEvent(payload: Create.Event.GroupsDisabled()),
                Create.CommittedEvent(payload: Create.Event.GroupsEnabled()),
                Create.CommittedEvent(payload: Create.Event.QuestionsDisabled()),
                Create.CommittedEvent(payload: Create.Event.QuestionsEnabled()),
                Create.CommittedEvent(payload: Create.Event.AnswersDeclaredInvalid()),
                Create.CommittedEvent(payload: Create.Event.AnswersDeclaredValid()),
            };

            optimizer = Create.InterviewEventStreamOptimizer();
        };

        Because of = () =>
            result = optimizer.RemoveEventsNotNeededToBeSent(eventStream);

        It should_remove_calculated_events = () =>
            result.ShouldContainOnly(new[]
            {
                questionAnswered,
            });

        private static InterviewEventStreamOptimizer optimizer;
        private static IReadOnlyCollection<CommittedEvent> eventStream;
        private static IReadOnlyCollection<CommittedEvent> result;
        private static CommittedEvent questionAnswered;
    }
}