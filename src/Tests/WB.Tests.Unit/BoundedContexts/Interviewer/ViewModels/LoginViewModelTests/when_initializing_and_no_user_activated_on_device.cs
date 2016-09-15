using System;
using System.Linq;

using Machine.Specifications;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_initializing_and_no_user_activated_on_device : LoginViewModelTestContext
    {
        Establish context = () =>
        {
            var interview = Substitute.For<IStatefulInterview>();
            interview.CreatedOnClient.Returns(true);
         
            viewModel = CreateLoginViewModel(
                viewModelNavigationService: ViewModelNavigationServiceMock.Object,
                interviewersPlainStorage: InterviewersPlainStorage.Object);
        };

        Because of = () => viewModel.Load();

        It should_redirect_to_finish_installation_page = () => 
            ViewModelNavigationServiceMock.Verify(x => x.NavigateTo<FinishInstallationViewModel>(), Times.Once);

        static LoginViewModel viewModel;
        static Mock<IViewModelNavigationService> ViewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();
        static Mock<IPlainStorage<InterviewerIdentity>> InterviewersPlainStorage = new Mock<IPlainStorage<InterviewerIdentity>>();
    }
}