using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Spec;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_deleted_interview_events : InterviewTestsContext
    {
         [NUnit.Framework.OneTimeSetUp] public void context () {
            eventContext = new EventContext();
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.Version == questionnaireVersion);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewerAssigned(userId, userId, DateTimeOffset.Now));
            interview.Apply(new InterviewStatusChanged(InterviewStatus.Deleted, "", DateTimeOffset.Now));

            command = Create.Command.SynchronizeInterviewEventsCommand(
               userId: userId, 
               questionnaireId: questionnaireId, 
               questionnaireVersion: questionnaireVersion,
               synchronizedEvents: eventsToPublish,
               createdOnClient: false);
             BecauseOf();
         }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() => interview.SynchronizeInterviewEvents(command);

        [NUnit.Framework.Test] public void should_raise_InterviewRestored_event () =>
          eventContext.ShouldContainEvent<InterviewRestored>(@event => @event.UserId == userId);

        [NUnit.Framework.Test] public void should_raise_InterviewStatusChanged_event () =>
          eventContext.ShouldContainEvent<InterviewStatusChanged>(@event => @event.Status == InterviewStatus.Restored);

        [NUnit.Framework.Test] public void should_raise_interview_received_by_supervisor_event () =>
          eventContext.ShouldContainEvent<InterviewReceivedBySupervisor>();

        [NUnit.Framework.Test] public void should_raise_all_passed_events () =>
             eventsToPublish.All(x => eventContext.Events.Any(publishedEvent => publishedEvent.Payload.Equals(x)));

        static EventContext eventContext;
        static readonly Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static long questionnaireVersion = 18;

        static Interview interview;

        static readonly IEvent[] eventsToPublish = new IEvent[]
        {
            new AnswersDeclaredInvalid(
                new Identity[0].ToDictionary<Identity, Identity, IReadOnlyList<FailedValidationCondition>>(question => question, question => new List<FailedValidationCondition>()), 
                DateTimeOffset.Now),
            new GroupsEnabled(new Identity[0], DateTimeOffset.Now)
        };
        static SynchronizeInterviewEventsCommand command;
    }
}
