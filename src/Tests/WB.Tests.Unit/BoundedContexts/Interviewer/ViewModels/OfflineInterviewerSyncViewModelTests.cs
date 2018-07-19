using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Plugin.Permissions.Abstractions;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels
{
    [TestOf(typeof(OfflineInterviewerSyncViewModel))]
    internal class OfflineInterviewerSyncViewModelTests
    {
        [Test]
        public async Task when_initialize_then_transfering_status_should_be_WaitingDevice()
        {
            //arrange
            var vm = Create.ViewModel.OfflineInterviewerSyncViewModel();
            //act
            await vm.Initialize();
            //assert
            Assert.That(vm.TransferingStatus, Is.EqualTo(TransferingStatus.WaitingDevice));
        }

        [Test]
        public void when_abort_then_transfering_status_should_be_Aborted()
        {
            //arrange
            var vm = Create.ViewModel.OfflineInterviewerSyncViewModel();
            //act
            vm.AbortCommand.Execute();
            //assert
            Assert.That(vm.TransferingStatus, Is.EqualTo(TransferingStatus.Aborted));
        }

        [Test]
        public void when_retry_then_should_nearby_connection_discovering_started()
        {
            //arrange
            var userId = Guid.Parse("11111111111111111111111111111111");
            var principal = Create.Service.InterviewerPrincipal(userId);
            var settings = Mock.Of<IEnumeratorSettings>(x => x.Endpoint == "http://google.com");
            var permissions = new Mock<IPermissionsService>();
            var nearbyConnection = new Mock<INearbyConnection>();
            nearbyConnection.SetupGet(x => x.Events).Returns(new Subject<INearbyEvent>());
            var vm = Create.ViewModel.OfflineInterviewerSyncViewModel(permissions: permissions.Object,
                nearbyConnection: nearbyConnection.Object, settings: settings, principal: principal);
            //act
            vm.RetryCommand.Execute();
            //assert
            Assert.That(vm.TransferingStatus, Is.EqualTo(TransferingStatus.WaitingDevice));
            permissions.Verify(x => x.AssureHasPermission(Permission.Location), Times.Once());
            nearbyConnection.Verify(x => x.StopDiscovery(), Times.Once);
            nearbyConnection.Verify(x => x.StartDiscovery(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void when_cancel_then_should_nearby_connection_discovering_be_stopped()
        {
            //arrange
            var nearbyConnection = new Mock<INearbyConnection>();
            nearbyConnection.SetupGet(x => x.Events).Returns(new Subject<INearbyEvent>());

            var viewModelNavigationService = new Mock<IViewModelNavigationService>();
            var vm = Create.ViewModel.OfflineInterviewerSyncViewModel(nearbyConnection: nearbyConnection.Object,
                viewModelNavigationService: viewModelNavigationService.Object);
            //act
            vm.CancelCommand.Execute();
            //assert
            nearbyConnection.Verify(x => x.StopDiscovery(), Times.Once);
            viewModelNavigationService.Verify(x => x.NavigateToDashboardAsync(null), Times.Once);
        }

        [Test]
        public void when_done_then_should_be_navigated_to_dashboard()
        {
            //arrange
            var viewModelNavigationService = new Mock<IViewModelNavigationService>();
            var vm = Create.ViewModel.OfflineInterviewerSyncViewModel(
                viewModelNavigationService: viewModelNavigationService.Object);
            //act
            vm.DoneCommand.Execute();
            //assert
            viewModelNavigationService.Verify(x => x.NavigateToDashboardAsync(null), Times.Once);
        }
    }
}
