using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveyManagementInterviewCommandValidatorTests
{
    [NUnit.Framework.TestOf(typeof(SurveyManagementInterviewCommandValidator))]
    internal class SurveyManagementInterviewCommandValidatorTestContext
    {
        protected static SurveyManagementInterviewCommandValidator CreateSurveyManagementInterviewCommandValidator(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage=null, int? limit=null)
        {
            return
                new SurveyManagementInterviewCommandValidator(
                    interviewSummaryStorage ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                    new InterviewPreconditionsServiceSettings(limit));
        }
    }
}
