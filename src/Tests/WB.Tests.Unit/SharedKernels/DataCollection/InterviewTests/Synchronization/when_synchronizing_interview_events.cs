using System;
using System.Linq;
using FluentAssertions;
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
    internal class when_synchronizing_interview_events : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            eventContext = new EventContext();
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.Version == questionnaireVersion);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, ""));
            interview.Apply(new InterviewerAssigned(userId, userId, DateTime.Now));

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

         public void BecauseOf() =>
            interview.SynchronizeInterviewEvents(command);

        [NUnit.Framework.Test] public void should_raise_all_passed_events () =>
            eventsToPublish.All(x => eventContext.Events.Any(publishedEvent => publishedEvent.Payload.Equals(x)));

        static EventContext eventContext;
        static readonly Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static long questionnaireVersion = 18;

        static Interview interview;

        static readonly IEvent[] eventsToPublish = new IEvent[] {new AnswersDeclaredInvalid(new Identity[0]), new GroupsEnabled(new Identity[0])};
        static SynchronizeInterviewEventsCommand command;
    }
}
