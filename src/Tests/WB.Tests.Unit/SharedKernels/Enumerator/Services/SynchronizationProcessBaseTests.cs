using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using List = NHibernate.Mapping.List;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services
{
    [TestOf(typeof(SynchronizationProcessBase))]
    public class SynchronizationProcessBaseTests
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

            var localInterviews = new InMemoryPlainStorage<InterviewView>();
            localInterviews.Store(Create.Entity.InterviewView(interviewId: reassingedInterviewId, responsibleId: oldAssignee));
            var busMock = new Mock<ILiteEventBus>();
            var interviewerInterviewAccessorMock = new Mock<IInterviewerInterviewAccessor>();

            var process = CreateSyncProcess(synchronizationService: syncService.Object,
                interviewViewRepository: localInterviews,
                eventBus: busMock.Object,
                interviewFactory: interviewerInterviewAccessorMock.Object);

            // Act
            await process.DownloadInterviewsAsync(new SynchronizationStatistics(), new Progress<SyncProgressInfo>(), CancellationToken.None);

            // Assert
            busMock.Verify(x => x.PublishCommittedEvents(interviewDetails));
            interviewerInterviewAccessorMock.Verify(x => x.RemoveInterview(reassingedInterviewId));
        }

        private static TestSynchronizationProcess CreateSyncProcess(ISynchronizationService synchronizationService = null, 
            IPlainStorage<InterviewView> interviewViewRepository = null, 
            IPrincipal principal = null,
            ILogger logger = null,
            IUserInteractionService userInteractionService = null,
            IInterviewerQuestionnaireAccessor questionnairesAccessor = null, 
            IInterviewerInterviewAccessor interviewFactory = null, 
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage = null, 
            IPlainStorage<InterviewFileView> imagesStorage = null, 
            CompanyLogoSynchronizer logoSynchronizer = null, 
            AttachmentsCleanupService cleanupService = null, 
            IPasswordHasher passwordHasher = null, IAssignmentsSynchronizer assignmentsSynchronizer = null, 
            IQuestionnaireDownloader questionnaireDownloader = null, 
            IHttpStatistician httpStatistician = null, 
            IAssignmentDocumentsStorage assignmentsStorage = null, 
            IAudioFileStorage audioFileStorage = null, 
            ITabletDiagnosticService diagnosticService = null, 
            IAuditLogSynchronizer auditLogSynchronizer = null, 
            IAuditLogService auditLogService = null, 
            ILiteEventBus eventBus = null, 
            IEnumeratorEventStorage eventStore = null)
        {
            return new TestSynchronizationProcess(
                synchronizationService ?? Create.Service.SynchronizationService(),
                interviewViewRepository ?? new InMemoryPlainStorage<InterviewView>(),
                principal ?? Create.Service.Principal(Guid.NewGuid()),
                logger ?? Mock.Of<ILogger>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                questionnairesAccessor ?? Mock.Of<IInterviewerQuestionnaireAccessor>(),
                interviewFactory ?? Mock.Of<IInterviewerInterviewAccessor>(),
                interviewMultimediaViewStorage ?? new InMemoryPlainStorage<InterviewMultimediaView>(),
                imagesStorage ?? new InMemoryPlainStorage<InterviewFileView>(),
                logoSynchronizer ?? new CompanyLogoSynchronizer(new InMemoryPlainStorage<CompanyLogo>(), Mock.Of<ISynchronizationService>()),
                cleanupService ?? Mock.Of<AttachmentsCleanupService>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                assignmentsSynchronizer ?? Mock.Of<IAssignmentsSynchronizer>(),
                questionnaireDownloader ?? Mock.Of<IQuestionnaireDownloader>(),
                httpStatistician ?? Mock.Of<IHttpStatistician>(),
                assignmentsStorage ?? Mock.Of<IAssignmentDocumentsStorage>(),
                audioFileStorage ?? Mock.Of<IAudioFileStorage>(),
                diagnosticService ?? Mock.Of<ITabletDiagnosticService>(),
                auditLogSynchronizer ?? Mock.Of<IAuditLogSynchronizer>(),
                auditLogService ?? Mock.Of<IAuditLogService>(),
                eventBus ?? Create.Service.LiteEventBus(),
                eventStore ?? Mock.Of<IEnumeratorEventStorage>());
        }

        private class TestSynchronizationProcess : SynchronizationProcessBase
        {
            public TestSynchronizationProcess(ISynchronizationService synchronizationService, IPlainStorage<InterviewView> interviewViewRepository, IPrincipal principal, ILogger logger, IUserInteractionService userInteractionService, IInterviewerQuestionnaireAccessor questionnairesAccessor, IInterviewerInterviewAccessor interviewFactory, IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage, IPlainStorage<InterviewFileView> imagesStorage, CompanyLogoSynchronizer logoSynchronizer, AttachmentsCleanupService cleanupService, IPasswordHasher passwordHasher, IAssignmentsSynchronizer assignmentsSynchronizer, IQuestionnaireDownloader questionnaireDownloader, IHttpStatistician httpStatistician, IAssignmentDocumentsStorage assignmentsStorage, IAudioFileStorage audioFileStorage, ITabletDiagnosticService diagnosticService, IAuditLogSynchronizer auditLogSynchronizer, IAuditLogService auditLogService, ILiteEventBus eventBus, IEnumeratorEventStorage eventStore) : base(synchronizationService, interviewViewRepository, principal, logger, userInteractionService, questionnairesAccessor, interviewFactory, interviewMultimediaViewStorage, imagesStorage, logoSynchronizer, cleanupService, assignmentsSynchronizer, questionnaireDownloader, httpStatistician, assignmentsStorage, audioFileStorage, diagnosticService, auditLogSynchronizer, auditLogService, eventBus, eventStore)
            {
            }

            public override Task Synchronize(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken, SynchronizationStatistics statistics)
            {
                throw new NotImplementedException();
            }

            protected override void CheckAfterStartSynchronization(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            protected override void UpdatePasswordOfResponsible(RestCredentials credentials)
            {
                throw new NotImplementedException();
            }

            protected override IReadOnlyCollection<InterviewView> GetInterviewsForUpload()
            {
                throw new NotImplementedException();
            }

            protected override int GetApplicationVersionCode()
            {
                throw new NotImplementedException();
            }
        }
    }
}
