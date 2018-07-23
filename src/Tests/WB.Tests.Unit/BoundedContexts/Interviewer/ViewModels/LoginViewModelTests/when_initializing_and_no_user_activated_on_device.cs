using System.Threading.Tasks;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_initializing_and_no_user_activated_on_device : LoginViewModelTestContext
    {
        [NUnit.Framework.Test]
        public async Task should_redirect_to_finish_installation_page()
        {
            Mock<IViewModelNavigationService> viewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();
            Mock<IPlainStorage<InterviewerIdentity>> interviewersPlainStorage = new Mock<IPlainStorage<InterviewerIdentity>>();

            var viewModel = CreateLoginViewModel(
                viewModelNavigationService: viewModelNavigationServiceMock.Object,
                interviewersPlainStorage: interviewersPlainStorage.Object);

            // Act
            viewModel.ViewCreated();

            // Assert
            viewModelNavigationServiceMock.Verify(x => x.NavigateToFinishInstallationAsync(), Times.Once);
        }
    }
}
