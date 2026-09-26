using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.Steps
{
    [TestOf(typeof(InterviewerUpdateApplication))]
    internal class InterviewerUpdateApplicationTests
    {
        [Test]
        public async Task when_interviewer_syncs_offline_without_cached_apk_then_shared_update_step_should_be_skipped()
        {
            var settings = Mock.Of<IInterviewerSettings>(x => x.AllowSyncWithHq == false);
            var synchronizationService = new Mock<ISynchronizationService>(MockBehavior.Strict);

            var step = CreateInterviewerUpdateApplication(settings, synchronizationService.Object);

            await step.ExecuteAsync();

            synchronizationService.Verify(x => x.IsAutoUpdateEnabledAsync(It.IsAny<CancellationToken>()), Times.Never);
            synchronizationService.Verify(x => x.GetLatestApplicationVersionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        private static InterviewerUpdateApplication CreateInterviewerUpdateApplication(
            IInterviewerSettings settings,
            ISynchronizationService synchronizationService)
        {
            var step = new InterviewerUpdateApplication(
                sortOrder: 1,
                synchronizationService: synchronizationService,
                logger: Mock.Of<ILogger>(),
                interviewerSettings: settings,
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
