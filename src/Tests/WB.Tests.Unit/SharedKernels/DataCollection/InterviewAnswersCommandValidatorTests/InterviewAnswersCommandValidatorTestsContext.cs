using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewAnswersCommandValidatorTests
{
    [Subject(typeof (InterviewAnswersCommandValidator))]
    internal class InterviewAnswersCommandValidatorTestsContext
    {
        protected static InterviewAnswersCommandValidator CreateInterviewAnswersCommandValidator()
        {
            return new InterviewAnswersCommandValidator();
        }
    }
}