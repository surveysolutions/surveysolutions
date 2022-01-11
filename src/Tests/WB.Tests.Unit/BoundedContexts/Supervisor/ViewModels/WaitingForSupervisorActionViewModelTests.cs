using System;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.ViewModels
{
    [TestOf(typeof(WaitingForSupervisorActionViewModel))]
    internal class WaitingForSupervisorActionViewModelTests : MvxIoCSupportingTest
    {
        [SetUp]
        public void SetUp()
        {
            base.Setup();
            Ioc.RegisterSingleton(Create.Fake.MvxMainThreadAsyncDispatcher());
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }

        [Test]
        public async Task when_getting_ui_items_and_view_model_has_last_visited_interview_id_then_view_model_should_have_specified_HighLightedItemIndex()
        {
            //arrange
            var factory = new Mock<IInterviewViewModelFactory>();
            factory.Setup(x => x.GetNew<DashboardSubTitleViewModel>())
                .Returns(new DashboardSubTitleViewModel());

            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var dashboardItemsAccessor = Mock.Of<IDashboardItemsAccessor>(x =>
                x.WaitingForSupervisorAction() == new[]
                {
                    Create.ViewModel.SupervisorDashboardInterviewViewModel(Guid.NewGuid(), null, null),
                    Create.ViewModel.SupervisorDashboardInterviewViewModel(interviewId, null, null)
                });
            var viewModel = Create.ViewModel.WaitingForSupervisorActionViewModel(dashboardItemsAccessor, factory.Object);
            viewModel.Prepare(interviewId);
            //act
            await viewModel.Initialize();
            //assert
            Assert.That(viewModel.HighLightedItemIndex, Is.EqualTo(2));
        }
    }
}
