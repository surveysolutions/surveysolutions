using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_initializing_and_no_user_activated_on_device : LoginViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var interview = Substitute.For<IStatefulInterview>();
         
            viewModel = CreateLoginViewModel(
                viewModelNavigationService: ViewModelNavigationServiceMock.Object,
                interviewersPlainStorage: InterviewersPlainStorage.Object);
            BecauseOf();
        }

        public void BecauseOf() => viewModel.Load();

        [NUnit.Framework.Test] public void should_redirect_to_finish_installation_page () => 
            ViewModelNavigationServiceMock.Verify(x => x.NavigateTo<FinishInstallationViewModel>(), Times.Once);

        static LoginViewModel viewModel;
        static Mock<IViewModelNavigationService> ViewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();
        static Mock<IPlainStorage<InterviewerIdentity>> InterviewersPlainStorage = new Mock<IPlainStorage<InterviewerIdentity>>();
    }
}
