#nullable enable
using System;
using System.Collections.Generic;
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
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerOnlineSynchronizationProcess : AbstractSynchronizationProcess
    {
        private readonly IInterviewerPrincipal principal;
        private readonly IPasswordHasher passwordHasher;
        private readonly IWorkspaceService workspaceService;
        private readonly IViewModelNavigationService navigationService;
        private readonly IInterviewerSynchronizationService synchronizationService;

        public InterviewerOnlineSynchronizationProcess(
            IOnlineSynchronizationService synchronizationService,
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewerPrincipal principal,
            ILogger logger,
            IPasswordHasher passwordHasher,
            IHttpStatistician httpStatistician,
            IAssignmentDocumentsStorage assignmentsStorage,
            IInterviewerSettings interviewerSettings,
            IAuditLogService auditLogService,
            IDeviceInformationService deviceInformationService,
            IUserInteractionService userInteractionService,
            IServiceLocator serviceLocator,
            IWorkspaceService workspaceService,
            IViewModelNavigationService navigationService) : base(synchronizationService, logger, httpStatistician, principal,
            interviewViewRepository, auditLogService, interviewerSettings, serviceLocator, deviceInformationService, userInteractionService,
            assignmentsStorage)
        {
            this.principal = principal;
            this.passwordHasher = passwordHasher;
            this.workspaceService = workspaceService;
            this.navigationService = navigationService;
            this.synchronizationService = synchronizationService;
        }

        protected override async Task CheckAfterStartSynchronization(CancellationToken cancellationToken)
        {
            if (RestCredentials == null)
            {
                throw new NullReferenceException("Rest credentials not set");
            }
            
            var currentSupervisorId = await this.synchronizationService.GetCurrentSupervisor(token: cancellationToken, credentials: this.RestCredentials);
            if (currentSupervisorId != this.principal.CurrentUserIdentity.SupervisorId)
            {
                this.UpdateSupervisorOfInterviewer(currentSupervisorId, this.RestCredentials.Login);
            }

            InterviewerApiView? interviewer = await this.synchronizationService.GetInterviewerAsync(this.RestCredentials, token: cancellationToken).ConfigureAwait(false);
            this.UpdateSecurityStampOfInterviewer(interviewer.SecurityStamp, this.RestCredentials.Login);
            this.UpdateWorkspaceInfo(interviewer.Workspaces);
        }

        protected override Task ChangeAndNavigateToNewDefaultWorkspaceAsync()
        {
            var workspaceView = workspaceService.GetAll().First();
            var interviewerIdentity = (InterviewerIdentity)principal.CurrentUserIdentity;
            interviewerIdentity.Workspace = workspaceView.Name;
            interviewerIdentity.SupervisorId = workspaceView.SupervisorId!.Value;
            principal.SaveInterviewer(interviewerIdentity);

            return navigationService.NavigateToDashboardAsync();
        }

        private void UpdateWorkspaceInfo(List<UserWorkspaceApiView> workspaces)
        {
            workspaceService.Save(workspaces.Select(w => new WorkspaceView()
            {
                Id = w.Name,
                DisplayName = w.DisplayName,
                SupervisorId = w.SupervisorId,
                Disabled = w.Disabled,
            }).ToArray());
        }

        private void UpdateSecurityStampOfInterviewer(string securityStamp, string name)
        {
            var localInterviewer = this.principal.GetInterviewerByName(name);
            if (localInterviewer.SecurityStamp != securityStamp)
            {
                localInterviewer.SecurityStamp = securityStamp;
                this.principal.SaveInterviewer(localInterviewer);
                this.principal.SignInWithHash(localInterviewer.Name, localInterviewer.PasswordHash, true);
            }
        }

        private void UpdateSupervisorOfInterviewer(Guid supervisorId, string name)
        {
            var localInterviewer = this.principal.GetInterviewerByName(name);
            localInterviewer.SupervisorId = supervisorId;
            this.principal.SaveInterviewer(localInterviewer);
            this.principal.SignInWithHash(localInterviewer.Name, localInterviewer.PasswordHash, true);
        }

        protected override void UpdatePasswordOfResponsible(RestCredentials credentials)
        {
            var localInterviewer = this.principal.GetInterviewerByName(credentials.Login);
            localInterviewer.PasswordHash = this.passwordHasher.Hash(credentials.Password);
            localInterviewer.Token = credentials.Token;

            this.principal.SaveInterviewer(localInterviewer);
            this.principal.SignIn(localInterviewer.Name, credentials.Password, true);
        }

        protected override string GetRequiredUpdate(string targetVersion, string appVersion)
        {
            return EnumeratorUIResources.UpgradeRequired 
                   + Environment.NewLine + string.Format(EnumeratorUIResources.HeadquartersVersion, targetVersion) 
                   + Environment.NewLine + string.Format(EnumeratorUIResources.InterviewerVersion, appVersion);

        }
    }
}
