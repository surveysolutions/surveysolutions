using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.DashboardViewModelTests
{
    public class when_signed_out : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            viewModel = CreateDashboardViewModel(
                principal: mockOfPrincipal.Object,
                viewModelNavigationService: mockOfViewModelNavigationService.Object
                );
        };

        Because of = () => viewModel.SignOutCommand.Execute();

        It should_be_called_signout_method_of_principal_service = () => mockOfPrincipal.Verify(_=>_.SignOut(), Times.Once);
        It should_be_navigated_to_login_view_model = () => mockOfViewModelNavigationService.Verify(_ => _.NavigateTo<LoginViewModel>(), Times.Once);
        
        private static DashboardViewModel viewModel;
        private static readonly Mock<IPrincipal> mockOfPrincipal = new Mock<IPrincipal>();
        private static readonly Mock<IViewModelNavigationService> mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
    }
}