using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public class SynchronizationProcess : SynchronizationProcessBase
    {
        private readonly ISupervisorSettings supervisorSettings;
        private readonly IPrincipal principal;
        private readonly IPlainStorage<SupervisorIdentity> supervisorsPlainStorage;
        private readonly IPasswordHasher passwordHasher;

        public SynchronizationProcess(ISynchronizationService synchronizationService,
            IPlainStorage<SupervisorIdentity> supervisorsPlainStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPrincipal principal,
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
            ISupervisorSettings supervisorSettings,
            IAuditLogSynchronizer auditLogSynchronizer,
            IAuditLogService auditLogService,
            ILiteEventBus eventBus,
            IEnumeratorEventStorage eventStore) : base(synchronizationService, interviewViewRepository, principal, logger,
            userInteractionService, questionnairesAccessor, interviewFactory, interviewMultimediaViewStorage, imagesStorage,
            logoSynchronizer, cleanupService, passwordHasher, assignmentsSynchronizer, questionnaireDownloader, httpStatistician,
            assignmentsStorage, audioFileStorage, diagnosticService, auditLogSynchronizer, auditLogService,
            eventBus, eventStore)
        {
            this.principal = principal;
            this.supervisorSettings = supervisorSettings;
            this.supervisorsPlainStorage = supervisorsPlainStorage;
            this.passwordHasher = passwordHasher;
        }


        protected override void CheckAfterStartSynchronization(CancellationToken cancellationToken)
        {

        }

        protected override void UpdatePasswordOfResponsible(RestCredentials credentials)
        {
            var localInterviewer = this.supervisorsPlainStorage.FirstOrDefault();
            localInterviewer.PasswordHash = this.passwordHasher.Hash(credentials.Password);
            localInterviewer.Token = credentials.Token;

            this.supervisorsPlainStorage.Store(localInterviewer);
            this.principal.SignIn(localInterviewer.Name, credentials.Password, true);
        }

        protected override int GetApplicationVersionCode()
        {
            return supervisorSettings.GetApplicationVersionCode();
        }

        protected override Task SyncronizeCensusQuestionnaires(IProgress<SyncProgressInfo> progress, SynchronizationStatistics statistics,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask; // supervisor is not support census
        }

        protected override Task<List<Guid>> FindObsoleteInterviewsAsync(List<Guid> localInterviewIds, IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<Guid>());
        }
    }
}
