using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ViewsTests.InterviewTests.InterviewSummaryViewFactoryTests
{
    internal class when_load_interview_summary_and_summary_does_not_exists : InterviewSummaryViewFactoryTestsContext
    {
        Establish context = () =>
        {
            var interviewSummaryReaderMock = new Mock<IReadSideRepositoryReader<InterviewSummary>>();
            interviewSummaryReaderMock.Setup(_ => _.GetById(interviewId)).Returns(() => null);
            factory = CreateFactory(interviewSummaryReader: interviewSummaryReaderMock.Object);
        };

        Because of = () => viewModel = factory.Load(interviewId);

        It should_view_model_be_null = () =>
            viewModel.ShouldBeNull();

        private static InterviewSummaryViewFactory factory;

        private static string interviewId = "11111111111111111111111111111111";

        private static InterviewSummary viewModel;
    }
}
