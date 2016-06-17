using Machine.Specifications;

using Moq;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    [Ignore("Roma, fix it in KP-7292")]
    internal class when_signed_out : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            viewModel = CreateDashboardViewModel(viewModelNavigationService: mockOfViewModelNavigationService.Object);
        };

        Because of = () => viewModel.SignOutCommand.Execute();
        
        It should_be_navigated_to_login_view_model = () => mockOfViewModelNavigationService.Verify(_ => _.SignOutAndNavigateToLogin(), Times.Once);
        
        private static DashboardViewModel viewModel;
        
        private static readonly Mock<IViewModelNavigationService> mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
    }
}