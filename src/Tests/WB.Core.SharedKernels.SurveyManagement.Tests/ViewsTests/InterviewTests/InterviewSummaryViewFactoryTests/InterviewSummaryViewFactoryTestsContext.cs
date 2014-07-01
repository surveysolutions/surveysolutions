using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ViewsTests.InterviewTests.InterviewSummaryViewFactoryTests
{
    internal class InterviewSummaryViewFactoryTestsContext
    {
        public static InterviewSummaryViewFactory CreateFactory(
            IReadSideRepositoryReader<InterviewSummary> interviewSummaryReader = null)
        {
            return
                new InterviewSummaryViewFactory(interviewSummaryReader ??
                                                Mock.Of<IReadSideRepositoryReader<InterviewSummary>>());
        }
    }
}
