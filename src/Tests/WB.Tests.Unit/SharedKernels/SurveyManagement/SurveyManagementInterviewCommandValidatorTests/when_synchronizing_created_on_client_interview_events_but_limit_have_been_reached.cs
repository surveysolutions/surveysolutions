using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveyManagementInterviewCommandValidatorTests
{
    internal class when_synchronizing_created_on_client_interview_events_but_limit_have_been_reached : SurveyManagementInterviewCommandValidatorTestContext
    {
        Establish context = () =>
        {
            surveyManagementInterviewCommandValidator =
              CreateSurveyManagementInterviewCommandValidator(limit: maxNumberOfInterviews,
                  interviewSummaryStorage:
                      Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(
                          _ => _.Query<int>(Moq.It.IsAny<Func<IQueryable<InterviewSummary>, int>>()) == 2));

            interview = Create.Interview();
        };

        Because of = () =>
          exception = Catch.Only<InterviewException>(() => surveyManagementInterviewCommandValidator.Validate(interview, Create.SynchronizeInterviewEventsCommand()));

        It should_raise_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_raise_InterviewException_with_type_InterviewLimitReached = () =>
           exception.ExceptionType.ShouldEqual(InterviewDomainExceptionType.InterviewLimitReached);

        It should_throw_exception_that_contains_such_words = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("max", "number", "interviews", "'" + maxNumberOfInterviews + "'", "reached");

        private static Interview interview;

        private static InterviewException exception;
        private static int maxNumberOfInterviews = 1;
        private static SurveyManagementInterviewCommandValidator surveyManagementInterviewCommandValidator;
    }
}
