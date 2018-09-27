using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerOfflineSynchronizationProcess : AbstractSynchronizationProcess
    {
        private readonly IOfflineSynchronizationService synchronizationService;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IInterviewerPrincipal principal;
        private readonly IInterviewerSettings interviewerSettings;
        public InterviewerOfflineSynchronizationProcess(
            IOfflineSynchronizationService synchronizationService,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IInterviewerPrincipal principal, 
            ILogger logger, 
            IHttpStatistician httpStatistician,
            IAssignmentDocumentsStorage assignmentsStorage,
            IAuditLogService auditLogService, 
            IInterviewerSettings interviewerSettings, 
            IServiceLocator serviceLocator,
            IUserInteractionService userInteractionService) :
            base(synchronizationService, logger, httpStatistician, principal,
                interviewViewRepository, auditLogService, interviewerSettings,
                serviceLocator, userInteractionService, assignmentsStorage)
        {
            this.synchronizationService = synchronizationService;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.principal = principal;
            this.interviewerSettings = interviewerSettings;
        }

        protected override Task<string> GetNewPasswordAsync() => Task.FromResult((string)null);

        protected override void WriteToAuditLogStartSyncMessage()
            => this.auditLogService.Write(new SynchronizationStartedAuditLogEntity(SynchronizationType.Offline));

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
            => throw new NotImplementedException("Update password by offline synchronization no supported now");
    }
}
