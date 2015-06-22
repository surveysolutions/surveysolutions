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
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_created_on_client_interview_events_but_limit_have_been_reached : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.Version == questionnaireVersion);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                =>
                repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion) == questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            SetupInstanceToMockedServiceLocator<IInterviewPreconditionsService>(
             Mock.Of<IInterviewPreconditionsService>(
                     _ => _.GetMaxAllowedInterviewsCount() == maxNumberOfInterviews && _.GetInterviewsCountAllowedToCreateUntilLimitReached() == 0));
            interview = Create.Interview();
        };

        Because of = () =>
          exception = Catch.Only<InterviewException>(() => interview.SynchronizeInterviewEvents(userId, questionnaireId, questionnaireVersion,
               InterviewStatus.Completed, eventsToPublish, true));

        It should_raise_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_raise_InterviewException_with_type_InterviewLimitReached = () =>
           exception.ExceptionType.ShouldEqual(InterviewDomainExceptionType.InterviewLimitReached);

        It should_throw_exception_that_contains_such_words = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("max", "number", "interviews", "'" + maxNumberOfInterviews + "'", "reached");

        Cleanup stuff = () =>
        {
            SetupInstanceToMockedServiceLocator<IInterviewPreconditionsService>(Mock.Of<IInterviewPreconditionsService>());
        };

        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static long questionnaireVersion = 18;

        private static Interview interview;

        private static InterviewException exception;
        private static long maxNumberOfInterviews = 1;

        private static object[] eventsToPublish = new object[] { new AnswersDeclaredInvalid(new Identity[0]), new GroupsEnabled(new Identity[0]) };
    }
}
