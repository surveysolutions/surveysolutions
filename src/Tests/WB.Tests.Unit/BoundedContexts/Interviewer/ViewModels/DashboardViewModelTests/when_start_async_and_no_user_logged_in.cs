using System;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using NSubstitute;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.DashboardViewModelTests
{
    internal class when_start_async_and_no_user_logged_in : DashboardViewModelTestsContext
    {
        Establish context = () =>
        {
            var principal = Mock.Of<IInterviewerPrincipal>();
            viewModel = CreateDashboardViewModel(principal: principal, synchronization: Substitute.For<SynchronizationViewModel> (), dashboardFactory: dashboardFactory.Object);
        };

        Because of = () => viewModel.Load();

        It should_not_call_GetInterviewerDashboardAsync = 
            () => dashboardFactory.Verify(m => m.GetInterviewerDashboardAsync(Moq.It.IsAny<Guid>()),Times.Never());

        static DashboardViewModel viewModel;
        static Mock<IInterviewerDashboardFactory> dashboardFactory = new Mock<IInterviewerDashboardFactory>();
        
    }
}
