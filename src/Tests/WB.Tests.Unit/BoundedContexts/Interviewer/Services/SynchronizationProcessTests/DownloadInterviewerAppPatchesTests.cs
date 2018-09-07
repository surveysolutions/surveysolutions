using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Plugin.Permissions.Abstractions;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [TestOf(typeof(DownloadInterviewerAppPatches))]
    internal class DownloadInterviewerAppPatchesTests
    {
        [Test]
        public async Task when_ExecuteAsync_should_ask_storage_permissions()
        {
            // arrange
            var mockOfPermissionsService = new Mock<IPermissionsService>();
            var step = CreateDownloadInterviewerAppPatches(permissions: mockOfPermissionsService.Object);
            // act
            await step.ExecuteAsync();
            // assert
            mockOfPermissionsService.Verify(x=>x.AssureHasPermission(Permission.Storage), Times.Once);
        }

        [Test]
        public async Task when_ExecuteAsync_and_interviewer_app_delta_exists_then_should_not_download_delta_from_server_again()
        {
            // arrange
            int? appVersion = 12345;
            var notExistingFile = "notExistingFile.delta";
            var existingFile = "existingFile.delta";
            var mockOfSupervisorSynchronization = new Mock<ISupervisorSynchronizationService>();
            mockOfSupervisorSynchronization.Setup(x => x.GetLatestInterviewerAppVersionAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(appVersion));
            mockOfSupervisorSynchronization
                .Setup(x => x.GetListOfInterviewerAppPatchesAsync(It.IsAny<CancellationToken>())).Returns(
                    Task.FromResult(new[]
                    {
                        Create.Entity.InterviewerApplicationPatchApiView(notExistingFile, notExistingFile),
                        Create.Entity.InterviewerApplicationPatchApiView(existingFile, existingFile),
                    }));
            var fileSystemAccessor = Mock.Of<IFileSystemAccessor>(x =>
                x.CombinePath(It.IsAny<string>(), notExistingFile) == notExistingFile &&
                x.CombinePath(It.IsAny<string>(), existingFile) == existingFile &&
                x.IsFileExists(notExistingFile) == false &&
                x.IsFileExists(existingFile) == true &&
                x.GetFileSize(existingFile) == 1);

            var mockOfSupervisorSettings = new Mock<ISupervisorSettings>();
            mockOfSupervisorSettings.Setup(x => x.GetApplicationVersionCode()).Returns(appVersion.Value);

            var step = CreateDownloadInterviewerAppPatches(
                synchronizationService: mockOfSupervisorSynchronization.Object,
                fileSystemAccessor: fileSystemAccessor,
                supervisorSettings: mockOfSupervisorSettings.Object);
            // act
            await step.ExecuteAsync();
            // assert
            mockOfSupervisorSynchronization.Verify(x => x.GetInterviewerApplicationPatchByNameAsync(notExistingFile, It.IsAny<CancellationToken>(), It.IsAny<IProgress<TransferProgress>>()), Times.Once);
            mockOfSupervisorSynchronization.Verify(x => x.GetInterviewerApplicationPatchByNameAsync(existingFile, It.IsAny<CancellationToken>(), It.IsAny<IProgress<TransferProgress>>()), Times.Never);
        }

        [Test]
        public async Task when_ExecuteAsync_and_interviewer_app_delta_exists_with_zero_length_then_should_download_delta_from_server_again()
        {
            // arrange
            int? appVersion = 12345;
            var zeroFileName = "existingFile.delta";
            var mockOfSupervisorSynchronization = new Mock<ISupervisorSynchronizationService>();
            mockOfSupervisorSynchronization.Setup(x => x.GetLatestInterviewerAppVersionAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(appVersion));
            mockOfSupervisorSynchronization
                .Setup(x => x.GetListOfInterviewerAppPatchesAsync(It.IsAny<CancellationToken>())).Returns(
                    Task.FromResult(new[]
                    {
                        new InterviewerApplicationPatchApiView {FileName = zeroFileName},
                    }));
            var fileSystemAccessor = Mock.Of<IFileSystemAccessor>(x =>
                x.CombinePath(It.IsAny<string>(), zeroFileName) == zeroFileName &&
                x.IsFileExists(zeroFileName) == true &&
                x.GetFileSize(zeroFileName) == 0);

            var mockOfSupervisorSettings = new Mock<ISupervisorSettings>();
            mockOfSupervisorSettings.Setup(x => x.GetApplicationVersionCode()).Returns(appVersion.Value);

            var step = CreateDownloadInterviewerAppPatches(
                synchronizationService: mockOfSupervisorSynchronization.Object,
                fileSystemAccessor: fileSystemAccessor,
                supervisorSettings: mockOfSupervisorSettings.Object);
            // act
            await step.ExecuteAsync();
            // assert
            mockOfSupervisorSynchronization.Verify(x => x.GetInterviewerApplicationPatchByNameAsync(zeroFileName, It.IsAny<CancellationToken>(), It.IsAny<IProgress<TransferProgress>>()), Times.Once);
        }

        [Test]
        public async Task when_ExecuteAsync_should_get_latest_interviewer_app_version_from_server()
        {
            // arrange
            int? version = 12345;

            var mockOfSupervisorSynchronization = new Mock<ISupervisorSynchronizationService>();
            mockOfSupervisorSynchronization.Setup(x => x.GetLatestInterviewerAppVersionAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(version));

            var mockOfSupervisorSettings = new Mock<ISupervisorSettings>();

            var step = CreateDownloadInterviewerAppPatches(
                synchronizationService: mockOfSupervisorSynchronization.Object,
                supervisorSettings: mockOfSupervisorSettings.Object);

            // act
            await step.ExecuteAsync();
            // assert
            mockOfSupervisorSynchronization.Verify(x => x.GetLatestInterviewerAppVersionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task when_ExecuteAsync_and_latest_interviewer_app_version_higher_then_supervisor_app_version_then_interviewer_app_patches_should_not_be_updated()
        {
            // arrange
            int? interviewerVersion = 12345;
            int supervisorVersion = 12344;

            var mockOfSupervisorSynchronization = new Mock<ISupervisorSynchronizationService>();
            mockOfSupervisorSynchronization.Setup(x => x.GetLatestInterviewerAppVersionAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(interviewerVersion));

            var mockOfSupervisorSettings = new Mock<ISupervisorSettings>();
            mockOfSupervisorSettings.Setup(x => x.GetApplicationVersionCode()).Returns(supervisorVersion);

            var step = CreateDownloadInterviewerAppPatches(
                synchronizationService: mockOfSupervisorSynchronization.Object,
                supervisorSettings: mockOfSupervisorSettings.Object);

            // act
            await step.ExecuteAsync();
            // assert
            mockOfSupervisorSynchronization.Verify(x => x.GetListOfInterviewerAppPatchesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task when_ExecuteAsync_and_old_patches_by_old_supervisor_version_exists_should_old_patches_be_removed()
        {
            // arrange
            int? version = 12345;
            int oldSupervisorVersion = 12344;

            var mockOfSupervisorSynchronization = new Mock<ISupervisorSynchronizationService>();
            mockOfSupervisorSynchronization.Setup(x => x.GetLatestInterviewerAppVersionAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(version));
            mockOfSupervisorSynchronization.Setup(x => x.GetListOfInterviewerAppPatchesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(Array.Empty<InterviewerApplicationPatchApiView>()));

            var mockOfSupervisorSettings = new Mock<ISupervisorSettings>();
            mockOfSupervisorSettings.Setup(x => x.GetApplicationVersionCode()).Returns(version.Value);

            var mockOfFileSystemAccessor = new Mock<IFileSystemAccessor>();
            mockOfFileSystemAccessor.Setup(x => x.GetDirectoriesInDirectory(It.IsAny<string>()))
                .Returns(new[] {version.ToString(), oldSupervisorVersion.ToString()});

            var step = CreateDownloadInterviewerAppPatches(
                synchronizationService: mockOfSupervisorSynchronization.Object,
                supervisorSettings: mockOfSupervisorSettings.Object,
                fileSystemAccessor: mockOfFileSystemAccessor.Object);

            // act
            await step.ExecuteAsync();
            // assert
            mockOfFileSystemAccessor.Verify(x => x.DeleteDirectory(It.Is<string>(y => y.Contains(oldSupervisorVersion.ToString()))), Times.Once);
        }

        private static DownloadInterviewerAppPatches CreateDownloadInterviewerAppPatches(
            ISupervisorSynchronizationService synchronizationService = null,
            ILogger logger = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IPermissionsService permissions = null,
            ISupervisorSettings supervisorSettings = null)
        {
            return new DownloadInterviewerAppPatches(0,
                logger: logger ?? Mock.Of<ILogger>(),
                synchronizationService: synchronizationService ?? Mock.Of<ISupervisorSynchronizationService>(),
                fileSystemAccessor: fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                permissions: permissions ?? Mock.Of<IPermissionsService>(), 
                supervisorSettings: supervisorSettings ?? Mock.Of<ISupervisorSettings>())
            {
                Context = Create.Entity.EnumeratorSynchonizationContext()
            };
        }
    }
}
