using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveyManagementInterviewCommandValidatorTests
{
    internal class when_synchronizing_created_on_client_interview_events_but_limit_have_been_reached : SurveyManagementInterviewCommandValidatorTestContext
    {
        [NUnit.Framework.Test] public void should_throw_exception_that_contains_such_words () {
            var summaries = new TestInMemoryWriter<InterviewSummary>();
            summaries.Store(Create.Entity.InterviewSummary(), "id1");
            summaries.Store(Create.Entity.InterviewSummary(), "id2");

            surveyManagementInterviewCommandValidator =
              CreateSurveyManagementInterviewCommandValidator(limit: maxNumberOfInterviews,
                  interviewSummaryStorage: summaries);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: Mock.Of<IQuestionnaireStorage>(), shouldBeInitialized: false);

            var exception = Assert.Throws<InterviewException>(() => surveyManagementInterviewCommandValidator.Validate(interview, Create.Command.SynchronizeInterviewEventsCommand()));
            exception.ExceptionType.Should().Be(InterviewDomainExceptionType.InterviewLimitReached);
            exception.Message.ToLower().ToSeparateWords().Should().Contain("limit", "interviews", "allowed", maxNumberOfInterviews.ToString());
        }


        private static StatefulInterview interview;

        private static int maxNumberOfInterviews = 1;
        private static SurveyManagementInterviewCommandValidator surveyManagementInterviewCommandValidator;
    }
}
