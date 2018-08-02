using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    public class SupervisorSynchronizationProcess : SynchronizationProcessBase
    {
        private readonly ISupervisorSettings supervisorSettings;
        private readonly IPrincipal principal;
        private readonly IPlainStorage<SupervisorIdentity> supervisorsPlainStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPasswordHasher passwordHasher;

        public SupervisorSynchronizationProcess(ISupervisorSynchronizationService synchronizationService,
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
            IEnumeratorEventStorage eventStore,
            IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository) : base(synchronizationService,
            interviewViewRepository, principal, logger,
            userInteractionService, questionnairesAccessor, interviewFactory, interviewMultimediaViewStorage,
            imagesStorage,
            logoSynchronizer, cleanupService, assignmentsSynchronizer, questionnaireDownloader,
            httpStatistician,
            assignmentsStorage, audioFileStorage, diagnosticService, auditLogSynchronizer, auditLogService,
            eventBus, eventStore, interviewSequenceViewRepository, supervisorSettings)
        {
            this.principal = principal;
            this.supervisorSettings = supervisorSettings;
            this.supervisorsPlainStorage = supervisorsPlainStorage;
            this.interviewViewRepository = interviewViewRepository;
            this.passwordHasher = passwordHasher;
        }

        public override async Task Synchronize(IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken, SynchronizationStatistics statistics)
        {
            var steps = ServiceLocator.Current.GetAllInstances<ISynchronizationStep>();

            var context = new EnumeratorSynchonizationContext
            {
                Progress = progress,
                CancellationToken = cancellationToken,
                Statistics = statistics
            };

            foreach (var step in steps.OrderBy(x => x.SortOrder))
            {
                cancellationToken.ThrowIfCancellationRequested();
                step.Context = context;
                await step.ExecuteAsync();
            }
        }


        protected override Task CheckAfterStartSynchronization(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override void UpdatePasswordOfResponsible(RestCredentials credentials)
        {
            var localSupervisor = this.supervisorsPlainStorage.FirstOrDefault();
            localSupervisor.PasswordHash = this.passwordHasher.Hash(credentials.Password);
            localSupervisor.Token = credentials.Token;

            this.supervisorsPlainStorage.Store(localSupervisor);
            this.principal.SignIn(localSupervisor.Name, credentials.Password, true);
        }

        protected override int GetApplicationVersionCode()
        {
            return supervisorSettings.GetApplicationVersionCode();
        }

        protected override Task SyncronizeCensusQuestionnaires(IProgress<SyncProgressInfo> progress,
            SynchronizationStatistics statistics,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask; // supervisor does not support census
        }

        protected override Task<List<Guid>> FindObsoleteInterviewsAsync(IEnumerable<InterviewView> localInterviews,
            IEnumerable<InterviewApiView> remoteInterviews,
            IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<Guid>());
        }

        protected override IReadOnlyCollection<InterviewView> GetInterviewsForUpload()
        {
            return this.interviewViewRepository.Where(interview =>
                interview.Status == InterviewStatus.ApprovedBySupervisor);
        }
    }
}
