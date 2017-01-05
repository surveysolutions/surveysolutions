using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveyManagementInterviewCommandValidatorTests
{
    internal class when_synchronizing_created_on_client_interview_events_but_limit_have_been_reached : SurveyManagementInterviewCommandValidatorTestContext
    {
        Establish context = () =>
        {
            var summaries = new TestInMemoryWriter<InterviewSummary>();
            summaries.Store(Create.Entity.InterviewSummary(), "id1");
            summaries.Store(Create.Entity.InterviewSummary(), "id2");

            surveyManagementInterviewCommandValidator =
              CreateSurveyManagementInterviewCommandValidator(limit: maxNumberOfInterviews,
                  interviewSummaryStorage: summaries);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: Mock.Of<IQuestionnaireStorage>(), shouldBeInitialized: false);
        };

        Because of = () =>
          exception = Catch.Only<InterviewException>(() => surveyManagementInterviewCommandValidator.Validate(interview, Create.Command.SynchronizeInterviewEventsCommand()));

        It should_raise_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_raise_InterviewException_with_type_InterviewLimitReached = () =>
           exception.ExceptionType.ShouldEqual(InterviewDomainExceptionType.InterviewLimitReached);

        It should_throw_exception_that_contains_such_words = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("limit", "interviews", "allowed", maxNumberOfInterviews.ToString(), "reached");

        private static StatefulInterview interview;

        private static InterviewException exception;
        private static int maxNumberOfInterviews = 1;
        private static SurveyManagementInterviewCommandValidator surveyManagementInterviewCommandValidator;
    }
}
