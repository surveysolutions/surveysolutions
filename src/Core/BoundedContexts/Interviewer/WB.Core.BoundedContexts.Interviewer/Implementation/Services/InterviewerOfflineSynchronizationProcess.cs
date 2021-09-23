using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerOfflineSynchronizationProcess : AbstractSynchronizationProcess
    {
        private readonly IOfflineSynchronizationService synchronizationService;
        private readonly IInterviewerPrincipal principal;
        private readonly IInterviewerSettings interviewerSettings;
        public InterviewerOfflineSynchronizationProcess(
            IOfflineSynchronizationService synchronizationService,
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewerPrincipal principal, 
            ILogger logger, 
            IHttpStatistician httpStatistician,
            IAssignmentDocumentsStorage assignmentsStorage,
            IAuditLogService auditLogService, 
            IInterviewerSettings interviewerSettings, 
            IDeviceInformationService deviceInformationService,
            IServiceLocator serviceLocator,
            IUserInteractionService userInteractionService) :
            base(synchronizationService, logger, httpStatistician, principal,
                interviewViewRepository, auditLogService, interviewerSettings,
                serviceLocator, deviceInformationService, userInteractionService, assignmentsStorage)
        {
            this.synchronizationService = synchronizationService;
            this.principal = principal;
            this.interviewerSettings = interviewerSettings;
        }

        protected override string GetRequiredUpdate(string targetVersion, string appVersion)
        {
            return EnumeratorUIResources.UpgradeRequired 
                      + Environment.NewLine + string.Format(EnumeratorUIResources.SupervisorVersion, targetVersion) 
                      + Environment.NewLine + string.Format(EnumeratorUIResources.InterviewerVersion, appVersion);
        }

        protected override Task<string?> GetNewPasswordAsync() => Task.FromResult(default(string));

        protected override void WriteToAuditLogStartSyncMessage()
            => this.auditLogService.Write(new SynchronizationStartedAuditLogEntity(SynchronizationType.Offline));

        protected override void OnSuccessfulSynchronization() => this.interviewerSettings.SetOfflineSynchronizationCompleted();
        protected override async Task CheckAfterStartSynchronization(CancellationToken cancellationToken)
        {
            if(this.RestCredentials == null)
                throw new NullReferenceException("Rest credentials not set");
            var currentSupervisorId = await this.synchronizationService.GetCurrentSupervisor(token: cancellationToken, credentials: this.RestCredentials);
            if (currentSupervisorId != ((InterviewerIdentity)this.principal.CurrentUserIdentity).SupervisorId)
            {
                this.UpdateSupervisorOfInterviewer(currentSupervisorId, this.RestCredentials.Login);
            }
        }
        
        protected override Task RefreshUserInfo(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task ChangeWorkspaceAndNavigateToItAsync()
            => throw new NotImplementedException("Remove workspace by offline synchronization no supported");

        private void UpdateSupervisorOfInterviewer(Guid supervisorId, string name)
        {
            var localInterviewer = this.principal.GetInterviewerByName(name);
            localInterviewer.SupervisorId = supervisorId;
            this.principal.SaveInterviewer(localInterviewer);

            this.principal.SignInWithHash(this.principal.CurrentUserIdentity.Name, this.principal.CurrentUserIdentity.PasswordHash, true);
        }

        protected override void UpdatePasswordOfResponsible(RestCredentials credentials) 
            => throw new NotImplementedException("Update password by offline synchronization no supported now");
    }
}
