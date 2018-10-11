using System;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Base;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.ViewModels
{
    [TestOf(typeof(WaitingForSupervisorActionViewModel))]
    internal class WaitingForSupervisorActionViewModelTests : MvxIoCSupportingTest
    {
        public WaitingForSupervisorActionViewModelTests()
        {
            base.Setup();

            Ioc.RegisterSingleton(Create.Fake.MvxMainThreadAsyncDispatcher());
        }

        [Test]
        public void when_getting_ui_items_and_view_model_has_last_visited_interview_id_then_view_model_should_have_specified_HighLightedItemIndex()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var dashboardItemsAccessor = Mock.Of<IDashboardItemsAccessor>(x =>
                x.WaitingForSupervisorAction() == new[]
                {
                    Create.ViewModel.SupervisorDashboardInterviewViewModel(Guid.NewGuid(), null, null),
                    Create.ViewModel.SupervisorDashboardInterviewViewModel(interviewId, null, null)
                });
            var viewModel = Create.ViewModel.WaitingForSupervisorActionViewModel(dashboardItemsAccessor);
            viewModel.Prepare(interviewId);
            //act
            viewModel.ViewAppeared();
            //assert
            Assert.That(viewModel.HighLightedItemIndex, Is.EqualTo(2));
        }
    }
}
