using Machine.Specifications;

using Moq;

using NSubstitute;

using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_initializing_and_no_user_activated_on_device : LoginViewModelTestContext
    {
        Establish context = () =>
        {
            var interview = Substitute.For<IStatefulInterview>();
            interview.CreatedOnClient.Returns(true);

            viewModel = CreateLoginViewModel(interviewer: null);
        };

        Because of = () => viewModel.Init();

        It should_redirect_to_finish_installation_page = () => 
            ViewModelNavigationServiceMock.Verify(x => x.NavigateTo<FinishInstallationViewModel>(), Times.Once);

        static LoginViewModel viewModel;
    }
}