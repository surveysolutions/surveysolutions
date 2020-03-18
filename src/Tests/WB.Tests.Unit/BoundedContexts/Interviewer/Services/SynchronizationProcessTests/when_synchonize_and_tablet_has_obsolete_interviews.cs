﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [TestOf(typeof(InterviewerDownloadInterviews))]
    public class when_synchonize_and_tablet_has_obsolete_interviews 
    {
        [Test]
        public async Task should_replace_own_interview_with_remote_one()
        {
            var obsoleteInterviewId = Id.g1;

            var syncService = new Mock<IInterviewerSynchronizationService>();
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

            syncService.Setup(x => x.GetInterviewDetailsAsync(obsoleteInterviewId, It.IsAny<IProgress<TransferProgress>>(), CancellationToken.None))
                .ReturnsAsync(serverEvents);

            var localInterviewsStorage = Create.Storage.InMemorySqlitePlainStorage<InterviewView>();
            localInterviewsStorage.Store(new InterviewView
            {
                InterviewId = obsoleteInterviewId,
                Id = obsoleteInterviewId.FormatGuid()
            });

            var interviewAccessorMock = new Mock<IInterviewsRemover>();

            var eventStore = new Mock<IEnumeratorEventStorage>();
            eventStore.Setup(x => x.GetLastEventKnownToHq(obsoleteInterviewId))
                .Returns(5);

            var eventBus = new Mock<IEventBus>();

            var syncProcess = Create.Service.InterviewerDownloadInterviews(synchronizationService: syncService.Object,
                interviewViewRepository: localInterviewsStorage,
                interviewsRemover: interviewAccessorMock.Object,
                eventStore: eventStore.Object,
                eventBus: eventBus.Object);

            // Act
            await syncProcess.ExecuteAsync();

            // Assert
            interviewAccessorMock.Verify(x => x.RemoveInterviews(It.IsAny<SynchronizationStatistics>(), It.IsAny<IProgress<SyncProgressInfo>>(), obsoleteInterviewId), Times.Once,
                "Interview that was changed on a server should be removed from tablet");
            eventBus.Verify(x => x.PublishCommittedEvents(serverEvents), Times.Once);
        }
    }
}
