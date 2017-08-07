using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_signed_out : DashboardViewModelTestContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            viewModel = CreateDashboardViewModel(viewModelNavigationService: mockOfViewModelNavigationService.Object);

            Because();
        }

        public void Because() => viewModel.SignOutCommand.Execute();

        [Test]
        public void should_be_navigated_to_login_view_model() 
            => mockOfViewModelNavigationService.Verify(_ => _.SignOutAndNavigateToLogin(), Times.Once);

        private static DashboardViewModel viewModel;

        private static readonly Mock<IViewModelNavigationService> mockOfViewModelNavigationService =
            new Mock<IViewModelNavigationService>();
    }
}