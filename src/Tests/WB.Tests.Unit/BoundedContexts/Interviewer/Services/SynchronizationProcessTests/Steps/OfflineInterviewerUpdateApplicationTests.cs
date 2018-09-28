using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Plugin.Permissions.Abstractions;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.Steps
{
    [TestOf(typeof(OfflineInterviewerUpdateApplication))]
    internal class OfflineInterviewerUpdateApplicationTests
    {
        [Test]
        public async Task when_interviewer_can_synchronize_with_hq_then_should_not_update_app_during_offline_sync()
        {
            // arrange
            var settings = Mock.Of<IInterviewerSettings>(x => x.AllowSyncWithHq == true);
            var mockOfSynchronizationService = new Mock<ISynchronizationService>();
            var step = CreateOfflineInterviewerUpdateApplication(interviewerSettings: settings, synchronizationService: mockOfSynchronizationService.Object);
            // act
            await step.ExecuteAsync();
            // assert
            mockOfSynchronizationService.Verify(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task when_interviewer_should_be_updated_then_should_apk_be_downloaded_and_saved_and_installation_run_and_app_closed()
        {
            // arrange
            var settings = Mock.Of<IInterviewerSettings>(x => x.AllowSyncWithHq == false &&
                                                              x.GetApplicationVersionCode() == 1);
            var mockOfSynchronizationService = new Mock<ISynchronizationService>();
            mockOfSynchronizationService.Setup(x =>
                x.GetApplicationAsync(It.IsAny<CancellationToken>(), It.IsAny<IProgress<TransferProgress>>())).Returns(Task.FromResult(new byte[] { 1, 2, 3 }));
            mockOfSynchronizationService.Setup(x =>
                x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult((int?) 2));

            var navigationService = new Mock<IViewModelNavigationService>();
            var fileSystemService = new Mock<IFileSystemAccessor>();
            var permissions = new Mock<IPermissionsService>();
            var step = CreateOfflineInterviewerUpdateApplication(interviewerSettings: settings,
                synchronizationService: mockOfSynchronizationService.Object,
                navigationService: navigationService.Object,
                fileSystemAccessor: fileSystemService.Object,
                permissions: permissions.Object);
            // act
            await step.ExecuteAsync();
            // assert

            permissions.Verify(x => x.AssureHasPermission(Permission.Storage), Times.Once);
            mockOfSynchronizationService.Verify(x => x.GetApplicationAsync(It.IsAny<CancellationToken>(), It.IsAny<IProgress<TransferProgress>>()), Times.Once);
            fileSystemService.Verify(x => x.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
            navigationService.Verify(x => x.InstallNewApp(It.IsAny<string>()), Times.Once);
            navigationService.Verify(x => x.CloseApplication(), Times.Once);
        }

        private OfflineInterviewerUpdateApplication CreateOfflineInterviewerUpdateApplication(
            ISynchronizationService synchronizationService = null,
            IInterviewerSettings interviewerSettings = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IPermissionsService permissions = null,
            IViewModelNavigationService navigationService = null)
        {
            var step = new OfflineInterviewerUpdateApplication(
                sortOrder: 1,
                synchronizationService: synchronizationService ?? Mock.Of<ISynchronizationService>(),
                logger: Mock.Of<ILogger>(),
                interviewerSettings: interviewerSettings ?? Mock.Of<IInterviewerSettings>(),
                fileSystemAccessor: fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                permissions: permissions ?? Mock.Of<IPermissionsService>(),
                navigationService: navigationService ?? Mock.Of<IViewModelNavigationService>(),
                pathUtils: Mock.Of<IPathUtils>());
            step.Context = new EnumeratorSynchonizationContext {Progress = new Progress<SyncProgressInfo>()};

            return step;
        }
    }
}
