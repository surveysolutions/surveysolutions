﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.SynchronizationSteps
{
    [TestOf(typeof(InterviewerDownloadInterviews))]
    public class InterviewerDownloadInterviewsTests
    {
        [Test]
        public async Task when_responsible_changed_should_remove_local_interview_and_download_it_again()
        {
            var reassingedInterviewId = Id.gA;
            var newAssignee = Id.gB;
            var oldAssignee = Id.gC;

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(x => x.GetInterviewsAsync(CancellationToken.None))
                .ReturnsAsync(new List<InterviewApiView>
                {
                    new InterviewApiView
                    {
                        Id = reassingedInterviewId,
                        ResponsibleId = newAssignee,
                        QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity()
                    }
                });
            syncService.Setup(x => x.CheckObsoleteInterviewsAsync(It.IsAny<List<ObsoletePackageCheck>>(), CancellationToken.None))
                .ReturnsAsync(new List<Guid>());

            List<CommittedEvent> interviewDetails = new List<CommittedEvent>();
            syncService.Setup(x => x.GetInterviewDetailsAsync(reassingedInterviewId, It.IsAny<IProgress<TransferProgress>>(), CancellationToken.None))
                .ReturnsAsync(interviewDetails);

            var localInterviews = Create.Storage.InMemorySqlitePlainStorage<InterviewView>();
            localInterviews.Store(Create.Entity.InterviewView(interviewId: reassingedInterviewId, responsibleId: oldAssignee));
            var busMock = new Mock<ILiteEventBus>();
            var interviewerInterviewAccessorMock = new Mock<IInterviewsRemover>();

            var process = Create.Service.InterviewerDownloadInterviews(synchronizationService: syncService.Object,
                interviewViewRepository: localInterviews,
                eventBus: busMock.Object,
                interviewsRemover: interviewerInterviewAccessorMock.Object);

            // Act
            await process.ExecuteAsync();

            // Assert
            busMock.Verify(x => x.PublishCommittedEvents(interviewDetails));
            interviewerInterviewAccessorMock.Verify(x => x.RemoveInterviews(It.IsAny<SynchronizationStatistics>(), It.IsAny<IProgress<SyncProgressInfo>>(), reassingedInterviewId));
        }

        [Test]
        public async Task when_on_server_in_download_list_exists_interview_alredy_been_on_tablet_but_in_current_moment_is_upsent_should_check_sequesnce_and_download_new_version()
        {
            var interviewId = Id.gA;
            var responsibleId = Id.gB;

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(x => x.GetInterviewsAsync(CancellationToken.None))
                .ReturnsAsync(new List<InterviewApiView>
                {
                    new InterviewApiView
                    {
                        Id = interviewId,
                        ResponsibleId = responsibleId,
                        QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(),
                        Sequence = 7
                    }
                });
            syncService.Setup(x => x.CheckObsoleteInterviewsAsync(It.IsAny<List<ObsoletePackageCheck>>(), CancellationToken.None))
                .ReturnsAsync(new List<Guid>());

            List<CommittedEvent> interviewDetails = new List<CommittedEvent>();
            syncService.Setup(x => x.GetInterviewDetailsAsync(interviewId, It.IsAny<IProgress<TransferProgress>>(), CancellationToken.None))
                .ReturnsAsync(interviewDetails);

            var localInterviews = Create.Storage.InMemorySqlitePlainStorage<InterviewView>();
            var busMock = new Mock<ILiteEventBus>();

            var localInterviewSequence = Create.Storage.InMemorySqlitePlainStorage<InterviewSequenceView, Guid>();
            localInterviewSequence.Store(Create.Entity.InterviewSequenceView(interviewId, 5));

            var process = Create.Service.InterviewerDownloadInterviews(synchronizationService: syncService.Object,
                interviewViewRepository: localInterviews,
                eventBus: busMock.Object,
                interviewSequenceViewRepository: localInterviewSequence);

            // Act
            await process.ExecuteAsync();

            // Assert
            syncService.Verify(x => x.GetInterviewDetailsAsync(interviewId, It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()));
            busMock.Verify(x => x.PublishCommittedEvents(interviewDetails));
        }

        [Test]
        public async Task when_on_server_in_download_list_exists_interview_alredy_been_on_tablet_but_in_current_moment_is_upsent_should_check_sequesnce_and_dont_download_if_version_is_same()
        {
            var interviewId = Id.gA;
            var responsibleId = Id.gB;

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(x => x.GetInterviewsAsync(CancellationToken.None))
                .ReturnsAsync(new List<InterviewApiView>
                {
                    new InterviewApiView
                    {
                        Id = interviewId,
                        ResponsibleId = responsibleId,
                        QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(),
                        Sequence = 7
                    }
                });
            syncService.Setup(x => x.CheckObsoleteInterviewsAsync(It.IsAny<List<ObsoletePackageCheck>>(), CancellationToken.None))
                .ReturnsAsync(new List<Guid>());

            List<CommittedEvent> interviewDetails = new List<CommittedEvent>();
            syncService.Setup(x => x.GetInterviewDetailsAsync(interviewId, It.IsAny<IProgress<TransferProgress>>(), CancellationToken.None))
                .ReturnsAsync(interviewDetails);

            var localInterviews = Create.Storage.InMemorySqlitePlainStorage<InterviewView>();
            var busMock = new Mock<ILiteEventBus>();

            var localInterviewSequence = Create.Storage.InMemorySqlitePlainStorage<InterviewSequenceView, Guid>();
            localInterviewSequence.Store(Create.Entity.InterviewSequenceView(interviewId, 7));

            var process = Create.Service.InterviewerDownloadInterviews(synchronizationService: syncService.Object,
                interviewViewRepository: localInterviews,
                eventBus: busMock.Object,
                interviewSequenceViewRepository: localInterviewSequence);

            // Act
            await process.ExecuteAsync();

            // Assert
            syncService.Verify(x => x.GetInterviewDetailsAsync(interviewId, It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()), Times.Never);
            busMock.Verify(x => x.PublishCommittedEvents(interviewDetails), Times.Never);
        }

        [Test]
        public async Task when_on_server_in_download_list_exists_interview_alredy_been_on_tablet_should_check_responsibe_and_dont_download_if_it_same()
        {
            var interviewId = Id.gA;
            var responsibleId = Id.gB;

            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(x => x.GetInterviewsAsync(CancellationToken.None))
                .ReturnsAsync(new List<InterviewApiView>
                {
                    new InterviewApiView
                    {
                        Id = interviewId,
                        ResponsibleId = responsibleId,
                        QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(),
                        Sequence = 7
                    }
                });
            syncService.Setup(x => x.CheckObsoleteInterviewsAsync(It.IsAny<List<ObsoletePackageCheck>>(), CancellationToken.None))
                .ReturnsAsync(new List<Guid>());

            List<CommittedEvent> interviewDetails = new List<CommittedEvent>();
            syncService.Setup(x => x.GetInterviewDetailsAsync(interviewId, It.IsAny<IProgress<TransferProgress>>(), CancellationToken.None))
                .ReturnsAsync(interviewDetails);

            var localInterviews = Create.Storage.InMemorySqlitePlainStorage<InterviewView>();
            localInterviews.Store(Create.Entity.InterviewView(interviewId: interviewId, responsibleId: responsibleId));
            var busMock = new Mock<ILiteEventBus>();

            var process = Create.Service.InterviewerDownloadInterviews(synchronizationService: syncService.Object,
                interviewViewRepository: localInterviews,
                eventBus: busMock.Object);

            // Act
            await process.ExecuteAsync();

            // Assert
            syncService.Verify(x => x.GetInterviewDetailsAsync(interviewId, It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()), Times.Never);
            busMock.Verify(x => x.PublishCommittedEvents(interviewDetails), Times.Never);
        }

        [Test]
        public async Task when_interview_is_not_marked_as_received_by_IN_but_it_is_on_interviewer()
        {
            var responsible = Id.g1;

            var localInterviews = Create.Storage.InMemorySqlitePlainStorage<InterviewView>();
            var interviewId = Id.gA;
            var interview = Create.Entity.InterviewView(interviewId: interviewId, responsibleId: responsible);
            localInterviews.Store(interview);
            
            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(x => x.GetInterviewsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InterviewApiView>
                {
                    new InterviewApiView
                    {
                        Id = interviewId,
                        ResponsibleId = responsible,
                        QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(),
                        Sequence = 7,
                        IsMarkedAsReceivedByInterviewer = false
                    }
                });

            syncService.Setup(x => x.CheckObsoleteInterviewsAsync(It.IsAny<List<ObsoletePackageCheck>>(), CancellationToken.None))
                .ReturnsAsync(new List<Guid>());

            var localInterviewSequence = Create.Storage.InMemorySqlitePlainStorage<InterviewSequenceView, Guid>();
            localInterviewSequence.Store(Create.Entity.InterviewSequenceView(interviewId, 7));

            var step = Create.Service.InterviewerDownloadInterviews(synchronizationService: syncService.Object,
                interviewViewRepository: localInterviews,
                interviewSequenceViewRepository: localInterviewSequence);

            // Act
            await step.ExecuteAsync();

            // Assert
            syncService.Verify(x => x.LogInterviewAsSuccessfullyHandledAsync(interviewId), Times.Once);
        }
    }
}
