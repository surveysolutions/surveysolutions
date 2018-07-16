using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;


namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public class SynchronizationProcess : SynchronizationProcessBase
    {
        private readonly IInterviewerSettings interviewerSettings;
        private readonly ISynchronizationMode synchronizationMode;
        private readonly IInterviewerPrincipal principal;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPasswordHasher passwordHasher;

        public SynchronizationProcess(ISynchronizationService synchronizationService,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewerPrincipal principal,
            ILogger logger,
            IUserInteractionService userInteractionService,
            IInterviewerQuestionnaireAccessor questionnairesAccessor,
            IInterviewerInterviewAccessor interviewFactory,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage,
            IPlainStorage<InterviewFileView> imagesStorage,
            CompanyLogoSynchronizer logoSynchronizer,
            AttachmentsCleanupService cleanupService,
            IPasswordHasher passwordHasher,
            IAssignmentsSynchronizer assignmentsSynchronizer,
            IQuestionnaireDownloader questionnaireDownloader,
            IHttpStatistician httpStatistician,
            IAssignmentDocumentsStorage assignmentsStorage,
            IAudioFileStorage audioFileStorage,
            ITabletDiagnosticService diagnosticService,
            IInterviewerSettings interviewerSettings,
            IAuditLogSynchronizer auditLogSynchronizer,
            IAuditLogService auditLogService,
            ILiteEventBus eventBus,
            IEnumeratorEventStorage eventStore,
            ISynchronizationMode synchronizationMode) : base(synchronizationService, interviewViewRepository, principal, logger,
            userInteractionService, questionnairesAccessor, interviewFactory, interviewMultimediaViewStorage, imagesStorage,
            logoSynchronizer, cleanupService, assignmentsSynchronizer, questionnaireDownloader, httpStatistician,
            assignmentsStorage, audioFileStorage, diagnosticService, auditLogSynchronizer, auditLogService,
            eventBus, eventStore)
        {
            this.principal = principal;
            this.interviewerSettings = interviewerSettings;
            this.synchronizationMode = synchronizationMode;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.interviewViewRepository = interviewViewRepository;
            this.passwordHasher = passwordHasher;
        }

        public override async Task Synchronize(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken, SynchronizationStatistics statistics)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.UploadInterviewsAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.assignmentsSynchronizer.SynchronizeAssignmentsAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.SyncronizeCensusQuestionnaires(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.CheckObsoleteQuestionnairesAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.DownloadInterviewsAsync(statistics, progress, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.logoSynchronizer.DownloadCompanyLogo(progress, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.auditLogSynchronizer.SynchronizeAuditLogAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.UpdateApplicationAsync(progress, cancellationToken);
        }

        protected override SynchronizationType SynchronizationType
        {
            get
            {
                var mode = synchronizationMode.GetMode();
                if (mode == SynchronizationMode.Offline)
                    return SynchronizationType.Offline;
                if (mode == SynchronizationMode.Online)
                    return SynchronizationType.Online;
                throw new ArgumentException($"Unknown synchronization mode: {mode}");
            }
        }

        protected override async void CheckAfterStartSynchronization(CancellationToken cancellationToken)
        {
            var currentSupervisorId = await this.synchronizationService.GetCurrentSupervisor(token: cancellationToken, credentials: this.restCredentials);
            
            if (currentSupervisorId != this.principal.CurrentUserIdentity.SupervisorId)
            {
                this.UpdateSupervisorOfInterviewer(currentSupervisorId);
            }
        }

        private void UpdateSupervisorOfInterviewer(Guid supervisorId)
        {
            var localInterviewer = this.interviewersPlainStorage.FirstOrDefault();
            localInterviewer.SupervisorId = supervisorId;
            this.interviewersPlainStorage.Store(localInterviewer);
            this.principal.SignInWithHash(localInterviewer.Name, localInterviewer.PasswordHash, true);
        }

        protected override void UpdatePasswordOfResponsible(RestCredentials credentials)
        {
            var localInterviewer = this.interviewersPlainStorage.FirstOrDefault();
            localInterviewer.PasswordHash = this.passwordHasher.Hash(credentials.Password);
            localInterviewer.Token = credentials.Token;

            this.interviewersPlainStorage.Store(localInterviewer);
            this.principal.SignIn(localInterviewer.Name, credentials.Password, true);
        }

        protected override int GetApplicationVersionCode()
        {
            return interviewerSettings.GetApplicationVersionCode();
        }

        protected override IReadOnlyCollection<InterviewView> GetInterviewsForUpload()
        {
            return this.interviewViewRepository.Where(interview => interview.Status == InterviewStatus.Completed);
        }
    }
}
