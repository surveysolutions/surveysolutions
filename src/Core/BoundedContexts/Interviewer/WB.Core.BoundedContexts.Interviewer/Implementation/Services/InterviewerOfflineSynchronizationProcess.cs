using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerOfflineSynchronizationProcess : OfflineSynchronizationProcessBase
    {
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IInterviewerPrincipal principal;
        private readonly IPasswordHasher passwordHasher;
        private readonly IInterviewerSettings interviewerSettings;
        public InterviewerOfflineSynchronizationProcess(
            IOfflineSynchronizationService synchronizationService,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IInterviewerPrincipal principal, 
            ILogger logger,
            IPasswordHasher passwordHasher,
            IAssignmentsSynchronizer assignmentsSynchronizer, 
            IHttpStatistician httpStatistician,
            IAssignmentDocumentsStorage assignmentsStorage, 
            IAuditLogSynchronizer auditLogSynchronizer,
            IAuditLogService auditLogService, 
            IInterviewerSettings interviewerSettings, 
            IServiceLocator serviceLocator) :
            base(synchronizationService, interviewViewRepository, principal, logger, assignmentsSynchronizer,
                httpStatistician, assignmentsStorage, auditLogSynchronizer, auditLogService, interviewerSettings,
                serviceLocator)
        {
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.principal = principal;
            this.passwordHasher = passwordHasher;
            this.interviewerSettings = interviewerSettings;
        }

        protected override void OnSuccesfullSynchronization() => this.interviewerSettings.SetOfflineSynchronizationCompleted();
        protected override async Task CheckAfterStartSynchronization(CancellationToken cancellationToken)
        {
            var currentSupervisorId = await this.synchronizationService.GetCurrentSupervisor(token: cancellationToken, credentials: this.RestCredentials);
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
    }
}
