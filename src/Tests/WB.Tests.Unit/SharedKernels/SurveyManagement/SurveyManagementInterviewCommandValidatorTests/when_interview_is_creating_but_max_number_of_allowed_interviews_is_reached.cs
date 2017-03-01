using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveyManagementInterviewCommandValidatorTests
{
    internal class when_interview_is_creating_but_max_number_of_allowed_interviews_is_reached : SurveyManagementInterviewCommandValidatorTestContext
    {
        private Establish context = () =>
        {
            var summaries = new TestInMemoryWriter<InterviewSummary>();
            summaries.Store(Create.Entity.InterviewSummary(), "id1");
            summaries.Store(Create.Entity.InterviewSummary(), "id2");

            surveyManagementInterviewCommandValidator =
                CreateSurveyManagementInterviewCommandValidator(limit: maxNumberOfInterviews,
                    interviewSummaryStorage: summaries);
        };

        Because of = () =>
            exception = Catch.Only<InterviewException>(() =>surveyManagementInterviewCommandValidator.Validate(null, Create.Command.CreateInterviewCommand()));

        It should_raise_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_raise_InterviewException_with_type_InterviewLimitReached = () =>
           exception.ExceptionType.ShouldEqual(InterviewDomainExceptionType.InterviewLimitReached);

        It should_throw_exception_that_contains_such_words = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("limit", "interviews", "allowed", maxNumberOfInterviews.ToString(), "reached");

        private static int maxNumberOfInterviews = 1;
        private static InterviewException exception;
        private static SurveyManagementInterviewCommandValidator surveyManagementInterviewCommandValidator;
    }
}
