using Machine.Specifications;

using Moq;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_signed_out : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            viewModel = CreateDashboardViewModel(viewModelNavigationService: mockOfViewModelNavigationService.Object);
        };

        Because of = () => viewModel.SignOutCommand.Execute();

        It should_be_called_signout_method_of_principal_service = () => mockOfPrincipal.Verify(_=>_.SignOutAsync(), Times.Once);
        It should_be_navigated_to_login_view_model = () => mockOfViewModelNavigationService.Verify(_ => _.NavigateToAsync<LoginViewModel>(), Times.Once);
        
        private static DashboardViewModel viewModel;
        
        private static readonly Mock<IViewModelNavigationService> mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
    }
}