using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewAnswersCommandValidatorTests
{
    internal class when_answer_on_single_question_and_interview_approved_by_hq : InterviewAnswersCommandValidatorTestsContext
    {
        Establish context = () =>
        {
            interview.Apply(Create.InterviewStatusChangedEvent(InterviewStatus.ApprovedByHeadquarters).Payload);
            commandValidator = CreateInterviewAnswersCommandValidator();
        };

        Because of = () => exception = Catch.Only<InterviewException>(
            () => commandValidator.Validate(interview,
                    Create.AnswerSingleOptionQuestionCommand(interviewId: interviewId, userId: responsibleId)));

        It should_raise_interviewException = () =>
            exception.ShouldNotBeNull();

        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Interview interview = Create.Interview(interviewId);
        private static InterviewException exception;
        private static InterviewAnswersCommandValidator commandValidator;
    }
}