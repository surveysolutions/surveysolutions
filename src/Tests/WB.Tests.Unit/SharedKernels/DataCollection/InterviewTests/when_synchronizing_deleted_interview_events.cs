using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_deleted_interview_events : InterviewTestsContext
    {
        Establish context = () =>
        {
            eventContext = new EventContext();
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.Version == questionnaireVersion);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion) == questionnaire);

            interview = Create.Interview(questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewerAssigned(userId, userId, DateTime.Now));
            interview.Apply(new InterviewStatusChanged(InterviewStatus.Deleted, ""));
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.SynchronizeInterviewEvents(userId, questionnaireId, questionnaireVersion,
               InterviewStatus.Completed, eventsToPublish, false);

        It should_raise_InterviewRestored_event = () =>
          eventContext.ShouldContainEvent<InterviewRestored>(@event => @event.UserId == userId);

        It should_raise_InterviewStatusChanged_event = () =>
          eventContext.ShouldContainEvent<InterviewStatusChanged>(@event => @event.Status == InterviewStatus.Restored);

        It should_raise_interview_received_by_supervisor_event = () =>
          eventContext.ShouldContainEvent<InterviewReceivedBySupervisor>();

        It should_raise_all_passed_events = () =>
             eventsToPublish.All(x => eventContext.Events.Any(publishedEvent => publishedEvent.Payload.Equals(x)));

        private static EventContext eventContext;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static long questionnaireVersion = 18;

        private static Interview interview;

        private static ILiteEvent[] eventsToPublish = new ILiteEvent[] { new AnswersDeclaredInvalid(new Identity[0]), new GroupsEnabled(new Identity[0]) };
    }
}
