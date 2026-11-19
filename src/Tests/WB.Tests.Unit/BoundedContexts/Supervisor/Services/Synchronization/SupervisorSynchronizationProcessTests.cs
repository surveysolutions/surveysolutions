using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services.Synchronization
{
    [TestOf(typeof(SupervisorSynchronizationProcess))]
    internal class SupervisorSynchronizationProcessTests
    {
        [TestCase(null, true)]
        [TestCase(99, false)]
        [TestCase(100, true)]
        [TestCase(101, true)]
        public async Task should_validate_version_before_refreshing_user_info(int? serverVersion, bool compatible)
        {
            using var cancellation = new CancellationTokenSource();
            var token = cancellation.Token;
            var synchronizationService = new Mock<ISupervisorSynchronizationService>();
            var versionChecked = false;
            synchronizationService.Setup(s => s.GetLatestApplicationVersionAsync(token))
                .Callback(() => versionChecked = true)
                .ReturnsAsync(serverVersion);
            // Stop at the next request so the test does not run unrelated sync steps.
            synchronizationService.Setup(s => s.GetSupervisorAsync(It.IsAny<RestCredentials>(), token))
                .Callback(() => Assert.That(versionChecked, Is.True))
                .ThrowsAsync(new OperationCanceledException());

            var principal = Mock.Of<ISupervisorPrincipal>(p => p.CurrentUserIdentity ==
                new SupervisorIdentity { Name = "supervisor", Workspace = "primary" });
            var process = new SupervisorSynchronizationProcess(
                synchronizationService.Object,
                Mock.Of<IPlainStorage<SupervisorIdentity>>(),
                Mock.Of<IPlainStorage<InterviewView>>(),
                principal,
                Mock.Of<ILogger>(),
                Mock.Of<IUserInteractionService>(),
                Mock.Of<IPasswordHasher>(),
                Mock.Of<IHttpStatistician>(),
                Mock.Of<IAssignmentDocumentsStorage>(),
                Mock.Of<ISupervisorSettings>(s => s.GetApplicationVersionCode() == 100),
                Mock.Of<IDeviceInformationService>(),
                Mock.Of<IAuditLogService>(),
                Mock.Of<IServiceLocator>(),
                Mock.Of<IWorkspaceService>(),
                Mock.Of<IPlainStorage<SupervisorIdentity>>(),
                Mock.Of<IViewModelNavigationService>());
            var progress = new Mock<IProgress<SyncProgressInfo>>();

            await process.SynchronizeAsync(progress.Object, token);

            synchronizationService.Verify(s => s.GetLatestApplicationVersionAsync(token), Times.Once);
            synchronizationService.Verify(s => s.GetSupervisorAsync(It.IsAny<RestCredentials>(), token),
                compatible ? Times.Once() : Times.Never());
            synchronizationService.Verify(s => s.IsAutoUpdateEnabledAsync(It.IsAny<CancellationToken>()), Times.Never);
            progress.Verify(p => p.Report(It.Is<SyncProgressInfo>(info =>
                info.Status == SynchronizationStatus.Fail &&
                info.Description == EnumeratorUIResources.NotSupportedServerSyncProtocolVersion)),
                compatible ? Times.Never() : Times.Once());
        }
    }
}
