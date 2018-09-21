using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
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

        private OfflineInterviewerUpdateApplication CreateOfflineInterviewerUpdateApplication(
            ISynchronizationService synchronizationService = null,
            IInterviewerSettings interviewerSettings = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IPermissionsService permissions = null,
            IViewModelNavigationService navigationService = null) => new OfflineInterviewerUpdateApplication(
            sortOrder: 1,
            synchronizationService: synchronizationService ?? Mock.Of<ISynchronizationService>(),
            logger: Mock.Of<ILogger>(),
            interviewerSettings: interviewerSettings ?? Mock.Of<IInterviewerSettings>(),
            fileSystemAccessor: fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
            permissions: permissions ?? Mock.Of<IPermissionsService>(),
            navigationService: navigationService ?? Mock.Of<IViewModelNavigationService>());
    }
}
