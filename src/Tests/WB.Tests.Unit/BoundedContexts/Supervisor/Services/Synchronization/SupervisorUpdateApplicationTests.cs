using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services.Synchronization
{
    [TestOf(typeof(SupervisorUpdateApplication))]
    internal class SupervisorUpdateApplicationTests
    {
        [Test]
        public async Task when_server_version_is_lower_with_autoupdate_disabled_should_throw_incompatible_version()
        {
            // arrange
            const int appVersion = 38141;
            const int serverVersion = 32228;

            var settings = Mock.Of<ISupervisorSettings>(x => x.GetApplicationVersionCode() == appVersion);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService.Setup(x => x.IsAutoUpdateEnabledAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            synchronizationService.Setup(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((int?)serverVersion);

            var step = CreateSupervisorUpdateApplication(settings: settings, synchronizationService: synchronizationService.Object);

            // act & assert
            var ex = Assert.ThrowsAsync<SynchronizationException>(async () => await step.ExecuteAsync());
            Assert.That(ex.Type, Is.EqualTo(SynchronizationExceptionType.NotSupportedServerSyncProtocolVersion));
        }

        [Test]
        public async Task when_server_version_is_lower_with_autoupdate_enabled_should_throw_incompatible_version()
        {
            // arrange
            const int appVersion = 38141;
            const int serverVersion = 32228;

            var settings = Mock.Of<ISupervisorSettings>(x => x.GetApplicationVersionCode() == appVersion);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService.Setup(x => x.IsAutoUpdateEnabledAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            synchronizationService.Setup(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((int?)serverVersion);

            var step = CreateSupervisorUpdateApplication(settings: settings, synchronizationService: synchronizationService.Object);

            // act & assert
            var ex = Assert.ThrowsAsync<SynchronizationException>(async () => await step.ExecuteAsync());
            Assert.That(ex.Type, Is.EqualTo(SynchronizationExceptionType.NotSupportedServerSyncProtocolVersion));
        }

        [Test]
        public async Task when_server_version_equals_app_version_should_not_throw()
        {
            // arrange
            const int appVersion = 38141;

            var settings = Mock.Of<ISupervisorSettings>(x => x.GetApplicationVersionCode() == appVersion);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService.Setup(x => x.IsAutoUpdateEnabledAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            synchronizationService.Setup(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((int?)appVersion);

            var step = CreateSupervisorUpdateApplication(settings: settings, synchronizationService: synchronizationService.Object);

            // act & assert - should not throw
            await step.ExecuteAsync();
        }

        [Test]
        public async Task when_server_version_is_null_should_not_throw()
        {
            // arrange
            const int appVersion = 38141;

            var settings = Mock.Of<ISupervisorSettings>(x => x.GetApplicationVersionCode() == appVersion);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService.Setup(x => x.IsAutoUpdateEnabledAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            synchronizationService.Setup(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((int?)null);

            var step = CreateSupervisorUpdateApplication(settings: settings, synchronizationService: synchronizationService.Object);

            // act & assert - should not throw when server version is unknown
            await step.ExecuteAsync();
        }

        private static SupervisorUpdateApplication CreateSupervisorUpdateApplication(
            ISupervisorSettings settings = null,
            ISynchronizationService synchronizationService = null)
        {
            var step = new SupervisorUpdateApplication(
                sortOrder: 1,
                synchronizationService: synchronizationService ?? Mock.Of<ISynchronizationService>(),
                logger: Mock.Of<ILogger>(),
                interviewerSettings: settings ?? Mock.Of<ISupervisorSettings>(),
                diagnosticService: Mock.Of<ITabletDiagnosticService>());
            step.Context = new EnumeratorSynchonizationContext
            {
                Progress = new Progress<SyncProgressInfo>(),
                Statistics = new SynchronizationStatistics()
            };
            return step;
        }
    }
}
