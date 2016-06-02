using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewTests.InterviewSummaryViewFactoryTests
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
