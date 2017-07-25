using System;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_created_on_client_interview_events : InterviewTestsContext
    {
        Establish context = () =>
        {
            eventContext = new EventContext();

            var questionnaireRepository =
                Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(),
                    Create.Entity.PlainQuestionnaire(
                        Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.StaticText())));

            interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository, userId: userId);
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => interview.SynchronizeInterviewEvents(
            Create.Command.SynchronizeInterviewEventsCommand(userId: userId,
                questionnaireId: questionnaireId, 
                questionnaireVersion: questionnaireVersion, 
                synchronizedEvents: eventsToPublish));

        It should_raise_InterviewOnClientCreated_event = () =>
          eventContext.ShouldContainEvent<InterviewCreated>(@event => @event.UserId == userId);

        It should_raise_interview_received_by_supervisor_event = () =>
            eventContext.ShouldContainEvent<InterviewReceivedBySupervisor>();

        It should_raise_all_passed_events = () =>
             eventsToPublish.All(x => eventContext.Events.Any(publishedEvent => publishedEvent.Payload.Equals(x)));

       static EventContext eventContext;
       static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
       static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
       static int questionnaireVersion = 18;

       static StatefulInterview interview;

       static readonly IEvent[] eventsToPublish = new IEvent[] { new AnswersDeclaredInvalid(new Identity[0]), new GroupsEnabled(new Identity[0]) };
    }
}
