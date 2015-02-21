using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_created_on_client_interview_events : InterviewTestsContext
    {
        Establish context = () =>
        {
            eventContext = new EventContext();
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.Version == questionnaireVersion);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                =>
                repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion) == questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);
            interview = new Interview();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.SynchronizeInterviewEvents(userId, questionnaireId, questionnaireVersion,
               InterviewStatus.Completed, eventsToPublish, true);

        It should_raise_InterviewOnClientCreated_event = () =>
          eventContext.ShouldContainEvent<InterviewOnClientCreated>(@event => @event.UserId == userId);

        It should_raise_all_passed_events = () =>
             eventContext.Events.Skip(1).Select(e => e.Payload).ShouldEqual(eventsToPublish);

        private static EventContext eventContext;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static long questionnaireVersion = 18;

        private static Interview interview;

        private static object[] eventsToPublish = new object[] { new AnswersDeclaredInvalid(new Identity[0]), new GroupsEnabled(new Identity[0]) };
    }
}
