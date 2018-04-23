using System.Threading.Tasks;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.PrefilledQuestionsViewModelTests
{
    internal class when_opening_not_existed_interview : PrefilledQuestionsViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public async Task should_logg_error_and_navigate_to_dashboard()
        {
            string notExistedInterviewId = "same id";

            var loggerMock = new Mock<ILogger>();
            var interviewRepositoryMock = new Mock<IStatefulInterviewRepository>();
            var navigationServiceMock = new Mock<IViewModelNavigationService>();

            var viewModel = CreatePrefilledQuestionsViewModel(
                interviewRepository: interviewRepositoryMock.Object,
                viewModelNavigationService: navigationServiceMock.Object,
                logger: loggerMock.Object);
            viewModel.Prepare(new InterviewViewModelArgs{InterviewId = notExistedInterviewId});

            // Act
            await viewModel.Initialize();
            
            // Assert
            interviewRepositoryMock.Verify(ns => ns.Get(notExistedInterviewId), Times.Once());
            loggerMock.Verify(ns => ns.Error(Moq.It.IsAny<string>(), null), Times.Once());
            navigationServiceMock.Verify(ns => ns.NavigateToDashboardAsync(null), Times.Once());
        }
    }
}
