using System;
using Moq;
using MvvmCross.Plugins.Messenger;
using MvvmCross.Test.Core;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels
{
    [TestOf(typeof(DashboardViewModel))]
    [TestFixture]
    internal class DashboardViewModelTests : MvxIoCSupportingTest
    {
        public DashboardViewModelTests()
        {
            base.Setup();
        }

        [Test]
        public void When_start_async_and_no_user_logged_in_Then_should_never_called_get_interviewer_dashboard_async_from_dashboard_factory()
        {
            // arrange
            var dashboardFactory = new Mock<IInterviewerDashboardFactory>();
            var viewModel = CreateDashboardViewModel(dashboardFactory: dashboardFactory.Object);
            
            //act
            viewModel.Load();

            //assert
            dashboardFactory.Verify(m => m.GetInterviewerDashboardAsync(Moq.It.IsAny<Guid>()), Times.Never());
        }

        [Test]
        public void When_execute_SynchronizationCommand_and_census_interview_creating_Then_should_not_be_called_synchronization_and_waiting_message_showed()
        {
            // arrange
            var mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
            mockOfViewModelNavigationService.SetupGet(x => x.HasPendingOperations).Returns(true);

            var mockOfSynchronizationViewModel = new Mock<SynchronizationViewModel>();
            var viewModel = CreateDashboardViewModel(
                viewModelNavigationService: mockOfViewModelNavigationService.Object,
                synchronization: mockOfSynchronizationViewModel.Object);

            //act
            viewModel.SynchronizationCommand.Execute();

            //assert
            mockOfViewModelNavigationService.Verify(m => m.ShowWaitMessage(), Times.Once);
            mockOfSynchronizationViewModel.Verify(m => m.Synchronize(), Times.Never);
        }

        private static DashboardViewModel CreateDashboardViewModel(
            IViewModelNavigationService viewModelNavigationService = null,
            IInterviewerDashboardFactory dashboardFactory = null,
            IInterviewerPrincipal principal = null,
            SynchronizationViewModel synchronization = null,
            IMvxMessenger messenger = null,
            ICommandService commandService = null)
        {
            return new DashboardViewModel(viewModelNavigationService: viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                 dashboardFactory: dashboardFactory ?? Mock.Of<IInterviewerDashboardFactory>(),
                 principal: principal ?? Mock.Of<IInterviewerPrincipal>(),
                 synchronization: synchronization ?? Substitute.For<SynchronizationViewModel>(),
                 messenger: messenger ?? Mock.Of<IMvxMessenger>(),
                 commandService: commandService ?? Mock.Of<ICommandService>());
        }
    }
}
