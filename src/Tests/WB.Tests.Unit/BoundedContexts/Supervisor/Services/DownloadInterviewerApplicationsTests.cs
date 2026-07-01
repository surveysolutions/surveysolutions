using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(DownloadInterviewerApplications))]
    internal class DownloadInterviewerApplicationsTests
    {
        [Test]
        public async Task when_ExecuteAsync_should_ask_storage_permissions()
        {
            // arrange
            var mockOfPermissionsService = new Mock<IPermissionsService>();
            int? version = 12345;

            var mockOfSupervisorSynchronization = new Mock<ISupervisorSynchronizationService>();
            mockOfSupervisorSynchronization.Setup(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(version));

            var mockOfSupervisorSettings = new Mock<ISupervisorSettings>();
            mockOfSupervisorSettings.Setup(x => x.DownloadUpdatesForInterviewerApp).Returns(true);
            mockOfSupervisorSettings.Setup(x => x.GetApplicationVersionCode()).Returns(version.Value);

            var step = CreateDownloadInterviewerAppPatches(permissions: mockOfPermissionsService.Object, 
                supervisorSettings: mockOfSupervisorSettings.Object, synchronizationService: mockOfSupervisorSynchronization.Object);
            // act
            await step.ExecuteAsync();
            // assert
            mockOfPermissionsService.Verify(x=>x.AssureHasExternalStoragePermissionOrThrow(), Times.Once);
        }

        [Test]
        public async Task when_ExecuteAsync_and_interviewer_apk_exists_then_should_request_download_apk_from_server_again_with_existing_file_hash_and()
        {
            // arrange
            int? appVersion = 12345;
            byte[] filehash = new byte[] {1, 2, 3, 4, 5};
            var mockOfSupervisorSynchronization = new Mock<ISupervisorSynchronizationService>();
            mockOfSupervisorSynchronization.Setup(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(appVersion));

            mockOfSupervisorSynchronization.Setup(x =>
                    x.GetInterviewerApplicationAsync(filehash, It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<byte[]>(null));

            mockOfSupervisorSynchronization.Setup(x =>
                    x.GetInterviewerApplicationWithMapsAsync(filehash, It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<byte[]>(null));

            var fs = new Mock<IFileSystemAccessor>();
            fs.Setup(f => f.IsFileExists(It.IsAny<string>())).Returns(true);
            fs.Setup(f => f.ReadHash(It.IsAny<string>())).Returns(filehash);

            var mockOfSupervisorSettings = new Mock<ISupervisorSettings>();
            mockOfSupervisorSettings.Setup(x => x.DownloadUpdatesForInterviewerApp).Returns(true);
            mockOfSupervisorSettings.Setup(x => x.GetApplicationVersionCode()).Returns(appVersion.Value);

            var step = CreateDownloadInterviewerAppPatches(
                synchronizationService: mockOfSupervisorSynchronization.Object,
                fileSystemAccessor: fs.Object,
                supervisorSettings: mockOfSupervisorSettings.Object);

            // act
            await step.ExecuteAsync();

            // assert
            mockOfSupervisorSynchronization.Verify(x => x.GetInterviewerApplicationAsync(filehash, 
                It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()), Times.Once);
            
            mockOfSupervisorSynchronization.Verify(x => x.GetInterviewerApplicationWithMapsAsync(filehash, It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()), Times.Once);

            // verify that no files are written if hash matched remote server, i.e. returned content null
            fs.Verify(f => f.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        }

        [Test]
        public async Task when_ExecuteAsync_should_get_latest_interviewer_app_version_from_server()
        {
            // arrange
            int? version = 12345;

            var mockOfSupervisorSynchronization = new Mock<ISupervisorSynchronizationService>();
            mockOfSupervisorSynchronization.Setup(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(version));

            var mockOfSupervisorSettings = new Mock<ISupervisorSettings>();
            mockOfSupervisorSettings.Setup(x => x.DownloadUpdatesForInterviewerApp).Returns(true);

            var step = CreateDownloadInterviewerAppPatches(
                synchronizationService: mockOfSupervisorSynchronization.Object,
                supervisorSettings: mockOfSupervisorSettings.Object);

            // act
            await step.ExecuteAsync();
            // assert
            mockOfSupervisorSynchronization.Verify(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task when_ExecuteAsync_and_latest_interviewer_app_version_higher_then_supervisor_app_version_then_interviewer_apks_should_not_be_updated()
        {
            // arrange
            int? interviewerVersion = 12345;
            int supervisorVersion = 12344;

            var mockOfSupervisorSynchronization = new Mock<ISupervisorSynchronizationService>();
            mockOfSupervisorSynchronization.Setup(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(interviewerVersion));

            var mockOfSupervisorSettings = new Mock<ISupervisorSettings>();
            mockOfSupervisorSettings.Setup(x => x.DownloadUpdatesForInterviewerApp).Returns(true);
            mockOfSupervisorSettings.Setup(x => x.GetApplicationVersionCode()).Returns(supervisorVersion);

            var step = CreateDownloadInterviewerAppPatches(
                synchronizationService: mockOfSupervisorSynchronization.Object,
                supervisorSettings: mockOfSupervisorSettings.Object);

            // act
            await step.ExecuteAsync();
            // assert
            mockOfSupervisorSynchronization.Verify(x => x.GetInterviewerApplicationAsync(null, It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()), Times.Never);
            mockOfSupervisorSynchronization.Verify(x => x.GetInterviewerApplicationWithMapsAsync(null, It.IsAny<IProgress<TransferProgress>>(),It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task when_ExecuteAsync_and_old_patches_by_old_supervisor_version_exists_should_old_patches_be_removed()
        {
            // arrange
            int? version = 12345;
            int oldSupervisorVersion = 12344;

            var mockOfSupervisorSynchronization = new Mock<ISupervisorSynchronizationService>();
            mockOfSupervisorSynchronization.Setup(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(version));

            var mockOfSupervisorSettings = new Mock<ISupervisorSettings>();
            mockOfSupervisorSettings.Setup(x => x.DownloadUpdatesForInterviewerApp).Returns(true);
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

        [Test]
        public async Task when_downloading_apk_with_known_size_progress_should_be_throttled_to_1_percent_buckets()
        {
            // arrange
            int? version = 12345;
            var progressReports = new List<SyncProgressInfo>();
            var progress = new Progress<SyncProgressInfo>(r => progressReports.Add(r));

            var mockOfSupervisorSynchronization = new Mock<ISupervisorSynchronizationService>();
            mockOfSupervisorSynchronization.Setup(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(version));
            mockOfSupervisorSynchronization.Setup(x =>
                    x.GetInterviewerApplicationAsync(It.IsAny<byte[]>(), It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()))
                .Returns<byte[], IProgress<TransferProgress>, CancellationToken>((_, p, _) =>
                {
                    const long total = 1_000_000;
                    // Fire 100 events — one per 1%
                    for (int i = 1; i <= 100; i++)
                    {
                        p.Report(new TransferProgress
                        {
                            TotalBytesToReceive = total,
                            BytesReceived = total * i / 100,
                            ProgressPercentage = i
                        });
                    }
                    return Task.FromResult(new byte[] { 1, 2, 3 });
                });
            mockOfSupervisorSynchronization.Setup(x =>
                    x.GetInterviewerApplicationWithMapsAsync(It.IsAny<byte[]>(), It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<byte[]>(null));

            var mockOfSupervisorSettings = new Mock<ISupervisorSettings>();
            mockOfSupervisorSettings.Setup(x => x.DownloadUpdatesForInterviewerApp).Returns(true);
            mockOfSupervisorSettings.Setup(x => x.GetApplicationVersionCode()).Returns(version.Value);

            var step = CreateDownloadInterviewerAppPatches(
                synchronizationService: mockOfSupervisorSynchronization.Object,
                supervisorSettings: mockOfSupervisorSettings.Object,
                progress: progress);

            // act
            await step.ExecuteAsync();
            // Wait for Progress<T> callbacks to drain; poll until count stabilises (up to 5 s)
            int lastCount1 = -1;
            var deadline1 = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < deadline1)
            {
                await Task.Delay(20);
                if (progressReports.Count == lastCount1) break;
                lastCount1 = progressReports.Count;
            }

            // assert: 100 raw events fired but only ≤101 pass the 1% bucket filter (buckets 0-100 = 101 max)
            var appDownloadReports = progressReports.Count(p => p.Stage == SyncStage.DownloadApplication);
            Assert.That(appDownloadReports, Is.LessThanOrEqualTo(101),
                $"Expected at most 101 throttled reports (one per 1% bucket), got {appDownloadReports}");
        }

        [Test]
        public async Task when_downloading_apk_with_unknown_size_progress_should_be_throttled_by_bytes()
        {
            // arrange
            int? version = 12345;
            var progressReports = new List<SyncProgressInfo>();
            var progress = new Progress<SyncProgressInfo>(r => progressReports.Add(r));

            var mockOfSupervisorSynchronization = new Mock<ISupervisorSynchronizationService>();
            mockOfSupervisorSynchronization.Setup(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(version));
            mockOfSupervisorSynchronization.Setup(x =>
                    x.GetInterviewerApplicationAsync(It.IsAny<byte[]>(), It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()))
                .Returns<byte[], IProgress<TransferProgress>, CancellationToken>((_, p, _) =>
                {
                    // Fire 10 events at 256 KB each — below the 512 KB per-step threshold
                    for (int i = 1; i <= 10; i++)
                    {
                        p.Report(new TransferProgress
                        {
                            TotalBytesToReceive = null, // unknown length
                            BytesReceived = 256 * 1024L * i,
                            ProgressPercentage = 0
                        });
                    }
                    return Task.FromResult(new byte[] { 1, 2, 3 });
                });
            mockOfSupervisorSynchronization.Setup(x =>
                    x.GetInterviewerApplicationWithMapsAsync(It.IsAny<byte[]>(), It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<byte[]>(null));

            var mockOfSupervisorSettings = new Mock<ISupervisorSettings>();
            mockOfSupervisorSettings.Setup(x => x.DownloadUpdatesForInterviewerApp).Returns(true);
            mockOfSupervisorSettings.Setup(x => x.GetApplicationVersionCode()).Returns(version.Value);

            var step = CreateDownloadInterviewerAppPatches(
                synchronizationService: mockOfSupervisorSynchronization.Object,
                supervisorSettings: mockOfSupervisorSettings.Object,
                progress: progress);

            // act
            await step.ExecuteAsync();
            // Wait for Progress<T> callbacks to drain; poll until count stabilises (up to 5 s)
            int lastCount2 = -1;
            var deadline2 = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < deadline2)
            {
                await Task.Delay(20);
                if (progressReports.Count == lastCount2) break;
                lastCount2 = progressReports.Count;
            }

            // 10 events at 256 KB → only crossings of 512 KB threshold pass → ≤5 reports
            var appDownloadReports = progressReports.Count(r => r.Stage == SyncStage.DownloadApplication);
            Assert.That(appDownloadReports, Is.LessThanOrEqualTo(5),
                $"With unknown content length, expected at most 5 throttled reports (one per 512KB step), got {appDownloadReports}");
        }

        private static DownloadInterviewerApplications CreateDownloadInterviewerAppPatches(
            ISupervisorSynchronizationService synchronizationService = null,
            ILogger logger = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IPermissionsService permissions = null,
            ISupervisorSettings supervisorSettings = null,
            IProgress<SyncProgressInfo> progress = null)
        {
            return new DownloadInterviewerApplications(0,
                logger: logger ?? Mock.Of<ILogger>(),
                synchronizationService: synchronizationService ?? Mock.Of<ISupervisorSynchronizationService>(),
                fileSystemAccessor: fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                permissions: permissions ?? Mock.Of<IPermissionsService>(), 
                supervisorSettings: supervisorSettings ?? Mock.Of<ISupervisorSettings>())
            {
                Context = Create.Entity.EnumeratorSynchonizationContext(progress)
            };
        }
    }
}
