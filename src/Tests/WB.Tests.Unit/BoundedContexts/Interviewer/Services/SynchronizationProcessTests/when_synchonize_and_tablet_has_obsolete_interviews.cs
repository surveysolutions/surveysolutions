using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [TestOf(typeof(SynchronizationProcess))]
    public class when_synchonize_and_tablet_has_obsolete_interviews 
    {
        [Test]
        public async Task should_replace_own_interview_with_remote_one()
        {
            var obsoleteInterviewId = Id.g1;

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(x => x.CheckObsoleteInterviewsAsync(It.Is<List<ObsoletePackageCheck>>(y => y.Any(o => o.InterviewId == obsoleteInterviewId)), CancellationToken.None))
                .ReturnsAsync(new List<Guid> {obsoleteInterviewId});
            syncService.Setup(x => x.GetCensusQuestionnairesAsync(CancellationToken.None))
                .ReturnsAsync(new List<QuestionnaireIdentity>());
            syncService.Setup(x => x.GetServerQuestionnairesAsync(CancellationToken.None))
                .ReturnsAsync(new List<QuestionnaireIdentity>());
            syncService.Setup(x => x.GetInterviewsAsync(CancellationToken.None))
                .ReturnsAsync(new List<InterviewApiView>
                {
                    new InterviewApiView
                    {
                        Id = obsoleteInterviewId
                    }
                });
            var serverEvents = new List<CommittedEvent>();

            syncService.Setup(x => x.GetInterviewDetailsAsync(obsoleteInterviewId, It.IsAny<Action<decimal, long, long>>(), CancellationToken.None))
                .ReturnsAsync(serverEvents);

            var localInterviewsStorage = new InMemoryPlainStorage<InterviewView>();
            localInterviewsStorage.Store(new InterviewView
            {
                InterviewId = obsoleteInterviewId,
                Id = obsoleteInterviewId.FormatGuid()
            });

            var interviewAccessorMock = new Mock<IInterviewerInterviewAccessor>();

            var eventStore = new Mock<IEnumeratorEventStorage>();
            eventStore.Setup(x => x.GetLastEventKnownToHq(obsoleteInterviewId))
                .Returns(5);

            var eventBus = new Mock<IEventBus>();

            var syncProcess = Create.Service.SynchronizationProcess(synchronizationService: syncService.Object,
                interviewViewRepository: localInterviewsStorage,
                interviewFactory: interviewAccessorMock.Object,
                interviewerEventStorage: eventStore.Object,
                eventBus: eventBus.Object);

            // Act
            await syncProcess.Synchronize(Mock.Of<IProgress<SyncProgressInfo>>(), CancellationToken.None, new SynchronizationStatistics());

            // Assert
            interviewAccessorMock.Verify(x => x.RemoveInterview(obsoleteInterviewId), Times.Once, "Interview that was changed on a server should be removed from tablet");
            eventBus.Verify(x => x.PublishCommittedEvents(serverEvents), Times.Once);
        }
    }
}
