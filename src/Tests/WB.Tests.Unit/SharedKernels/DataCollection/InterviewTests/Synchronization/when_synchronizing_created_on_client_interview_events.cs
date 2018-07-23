using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Spec;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_created_on_client_interview_events : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            eventContext = new EventContext();

            var questionnaireRepository =
                Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(),
                    Create.Entity.PlainQuestionnaire(
                        Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.StaticText())));

            interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository, userId: userId);
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() => interview.SynchronizeInterviewEvents(
            Create.Command.SynchronizeInterviewEventsCommand(userId: userId,
                questionnaireId: questionnaireId, 
                questionnaireVersion: questionnaireVersion, 
                synchronizedEvents: eventsToPublish));

        [NUnit.Framework.Test] public void should_raise_InterviewOnClientCreated_event () =>
          eventContext.ShouldContainEvent<InterviewCreated>(@event => @event.UserId == userId);

        [NUnit.Framework.Test] public void should_raise_interview_received_by_supervisor_event () =>
            eventContext.ShouldContainEvent<InterviewReceivedBySupervisor>();

        [NUnit.Framework.Test] public void should_raise_all_passed_events () =>
             eventsToPublish.All(x => eventContext.Events.Any(publishedEvent => publishedEvent.Payload.Equals(x)));

       static EventContext eventContext;
       static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
       static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
       static int questionnaireVersion = 18;

       static StatefulInterview interview;

       static readonly IEvent[] eventsToPublish = new IEvent[]
       {
           new AnswersDeclaredInvalid(
               new Identity[0].ToDictionary<Identity, Identity, IReadOnlyList<FailedValidationCondition>>(question => question, question => new List<FailedValidationCondition>()), 
               DateTimeOffset.Now),
           new GroupsEnabled(new Identity[0], DateTimeOffset.Now)
       };
    }
}
