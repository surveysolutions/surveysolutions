using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerSynchronizationProcess : SynchronizationProcessBase
    {
        private readonly IInterviewerSettings interviewerSettings;
        private readonly ISynchronizationMode synchronizationMode;
        private readonly IInterviewerPrincipal principal;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IPasswordHasher passwordHasher;
        private readonly IInterviewerSynchronizationService interviewerSynchronizationService;

        public InterviewerSynchronizationProcess(
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewerPrincipal principal,
            ILogger logger,
            IUserInteractionService userInteractionService,
            IPasswordHasher passwordHasher,
            IAssignmentsSynchronizer assignmentsSynchronizer,
            IHttpStatistician httpStatistician,
            IAssignmentDocumentsStorage assignmentsStorage,
            IInterviewerSettings interviewerSettings,
            IAuditLogSynchronizer auditLogSynchronizer,
            IAuditLogService auditLogService,
            ISynchronizationMode synchronizationMode,
            IInterviewerSynchronizationService interviewerSynchronizationService) 
            : base(interviewerSynchronizationService, interviewViewRepository, principal, logger,
            userInteractionService, assignmentsSynchronizer, httpStatistician,
            assignmentsStorage, auditLogSynchronizer, auditLogService, interviewerSettings)
        {
            this.principal = principal;
            this.interviewerSettings = interviewerSettings;
            this.synchronizationMode = synchronizationMode;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.passwordHasher = passwordHasher;
            this.interviewerSynchronizationService = interviewerSynchronizationService;
        }

        public override async Task Synchronize(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken, SynchronizationStatistics statistics)
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

        protected override void OnSuccesfullSynchronization()
        {
            if(this.synchronizationMode.GetMode() == SynchronizationMode.Offline)
                this.interviewerSettings.SetOfflineSynchronizationCompleted();
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

        protected override async Task CheckAfterStartSynchronization(CancellationToken cancellationToken){

            var currentSupervisorId = await this.interviewerSynchronizationService.GetCurrentSupervisor(token: cancellationToken, credentials: this.RestCredentials);
            if (currentSupervisorId != this.principal.CurrentUserIdentity.SupervisorId)
            {
                this.UpdateSupervisorOfInterviewer(currentSupervisorId);
            }

            if (SynchronizationType == SynchronizationType.Online)
            {
                var interviewer = await this.interviewerSynchronizationService
                    .GetInterviewerAsync(this.RestCredentials, token: cancellationToken).ConfigureAwait(false);
                UpdateSecurityStampOfInterviewer(interviewer.SecurityStamp);
            }
        }

        private void UpdateSecurityStampOfInterviewer(string securityStamp)
        {
            var localInterviewer = this.interviewersPlainStorage.FirstOrDefault();
            if (localInterviewer.SecurityStamp != securityStamp)
            {
                localInterviewer.SecurityStamp = securityStamp;
                this.interviewersPlainStorage.Store(localInterviewer);
                this.principal.SignInWithHash(localInterviewer.Name, localInterviewer.PasswordHash, true);
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
    }
}
