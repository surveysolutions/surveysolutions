using Moq;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.PrefilledQuestionsViewModelTests
{
    internal class when_opening_not_existed_interview : PrefilledQuestionsViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            loggerMock = new Mock<ILogger>();
            interviewRepositoryMock = new Mock<IStatefulInterviewRepository>();
            navigationServiceMock = new Mock<IViewModelNavigationService>();

            viewModel = CreatePrefilledQuestionsViewModel(
                interviewRepository: interviewRepositoryMock.Object,
                viewModelNavigationService: navigationServiceMock.Object,
                logger: loggerMock.Object);
            viewModel.Init(notExistedInterviewId);
            BecauseOf();
        }

        public void BecauseOf() => viewModel.Load();

        [NUnit.Framework.Test] public void should_check_interview_in_repository () =>
            interviewRepositoryMock.Verify(ns => ns.Get(notExistedInterviewId), Times.Once());

        [NUnit.Framework.Test] public void should_logged_error () =>
            loggerMock.Verify(ns => ns.Error(Moq.It.IsAny<string>(), null), Times.Once());

        [NUnit.Framework.Test] public void should_navigate_to_dashboard () =>
            navigationServiceMock.Verify(ns => ns.NavigateToDashboard(null), Times.Once());


        private static string notExistedInterviewId = "same id";
        private static PrefilledQuestionsViewModel viewModel;
        private static Mock<ILogger> loggerMock;
        private static Mock<IStatefulInterviewRepository> interviewRepositoryMock;
        private static Mock<IViewModelNavigationService> navigationServiceMock;
    }
}
