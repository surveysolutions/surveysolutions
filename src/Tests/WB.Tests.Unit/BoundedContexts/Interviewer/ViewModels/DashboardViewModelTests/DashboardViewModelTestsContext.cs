using Machine.Specifications;
using Moq;
using MvvmCross.Plugins.Messenger;
using MvvmCross.Test.Core;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.DashboardViewModelTests
{
    [Subject(typeof(DashboardViewModel))]
    internal class DashboardViewModelTestsContext : MvxIoCSupportingTest
    {
        protected DashboardViewModelTestsContext()
        {
            base.Setup();
        }

        protected static DashboardViewModel CreateDashboardViewModel(
            IViewModelNavigationService viewModelNavigationService = null,
            IInterviewerDashboardFactory dashboardFactory = null,
            IInterviewerPrincipal principal = null,
            SynchronizationViewModel synchronization = null,
            IMvxMessenger messenger = null,
            ICommandService commandService = null)
        {
            return new DashboardViewModel(viewModelNavigationService : viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                 dashboardFactory : dashboardFactory ?? Mock.Of<IInterviewerDashboardFactory>(),
                 principal: principal ?? Mock.Of<IInterviewerPrincipal>(),
                 synchronization: synchronization,
                 messenger : messenger ?? Mock.Of<IMvxMessenger>(),
                 commandService: commandService ?? Mock.Of<ICommandService>());
        }
    }
}
