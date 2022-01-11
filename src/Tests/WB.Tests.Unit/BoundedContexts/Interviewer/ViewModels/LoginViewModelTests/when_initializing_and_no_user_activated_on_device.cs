using System.Threading.Tasks;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_initializing_and_no_user_activated_on_device : LoginViewModelTestContext
    {
        [NUnit.Framework.Test]
        public void should_redirect_to_finish_installation_page()
        {
            Mock<IViewModelNavigationService> viewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();
            Mock<IInterviewerPrincipal> interviewerPrincipal = new Mock<IInterviewerPrincipal>();

            var viewModel = CreateLoginViewModel(
                viewModelNavigationService: viewModelNavigationServiceMock.Object,
                principal: interviewerPrincipal.Object);

            // Act
            viewModel.ViewCreated();

            // Assert
            viewModelNavigationServiceMock.Verify(x => x.NavigateToFinishInstallationAsync(), Times.Once);
        }
    }
}
