using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveyManagementInterviewCommandValidatorTests
{
    internal class when_interview_is_creating_but_max_number_of_allowed_interviews_is_reached : SurveyManagementInterviewCommandValidatorTestContext
    {
        private Establish context = () =>
        {
            surveyManagementInterviewCommandValidator =
                CreateSurveyManagementInterviewCommandValidator(limit: maxNumberOfInterviews,
                    interviewSummaryStorage:
                        Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(
                            _ => _.Query<int>(Moq.It.IsAny<Func<IQueryable<InterviewSummary>, int>>()) == 2));
        };

        Because of = () =>
            exception = Catch.Only<InterviewException>(() =>surveyManagementInterviewCommandValidator.Validate(null, Create.CreateInterviewCommand()));

        It should_raise_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_raise_InterviewException_with_type_InterviewLimitReached = () =>
           exception.ExceptionType.ShouldEqual(InterviewDomainExceptionType.InterviewLimitReached);

        It should_throw_exception_that_contains_such_words = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("max", "number", "interviews", "'" + maxNumberOfInterviews + "'", "reached");

        private static int maxNumberOfInterviews = 1;
        private static InterviewException exception;
        private static SurveyManagementInterviewCommandValidator surveyManagementInterviewCommandValidator;
    }
}
