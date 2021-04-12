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
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    public class SupervisorSynchronizationProcess : AbstractSynchronizationProcess
    {
        private readonly ISupervisorSynchronizationService synchronizationService;
        private readonly ISupervisorPrincipal principal;
        private readonly IPlainStorage<SupervisorIdentity> supervisorsPlainStorage;
        private readonly IPasswordHasher passwordHasher;
        private readonly IWorkspaceService workspaceService;
        private readonly IPlainStorage<SupervisorIdentity> supervisorPlainStorage;
        private readonly IViewModelNavigationService navigationService;

        public SupervisorSynchronizationProcess(
            ISupervisorSynchronizationService synchronizationService,
            IPlainStorage<SupervisorIdentity> supervisorsPlainStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            ISupervisorPrincipal principal,
            ILogger logger,
            IUserInteractionService userInteractionService,
            IPasswordHasher passwordHasher,
            IHttpStatistician httpStatistician,
            IAssignmentDocumentsStorage assignmentsStorage,
            ISupervisorSettings supervisorSettings,
            IDeviceInformationService deviceInformationService,
            IAuditLogService auditLogService,
            IServiceLocator serviceLocator,
            IWorkspaceService workspaceService,
            IPlainStorage<SupervisorIdentity> supervisorPlainStorage,
            IViewModelNavigationService navigationService) : base(synchronizationService, logger, httpStatistician, principal,
            interviewViewRepository, auditLogService, supervisorSettings, serviceLocator, deviceInformationService, userInteractionService,
            assignmentsStorage)
        {
            this.synchronizationService = synchronizationService;
            this.principal = principal;
            this.supervisorsPlainStorage = supervisorsPlainStorage;
            this.passwordHasher = passwordHasher;
            this.workspaceService = workspaceService;
            this.supervisorPlainStorage = supervisorPlainStorage;
            this.navigationService = navigationService;
        }

        protected override Task CheckAfterStartSynchronization(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        protected override async Task RefreshUserInfo(CancellationToken cancellationToken)
        {
            if (RestCredentials == null)
            {
                throw new NullReferenceException("Rest credentials not set");
            }
            
            SupervisorApiView supervisor = await this.synchronizationService.GetSupervisorAsync(this.RestCredentials, token: cancellationToken).ConfigureAwait(false);
            if (supervisor.Workspaces.Count == 0)
                throw new NooneWorkspaceFoundException();
            this.UpdateWorkspaceInfo(supervisor.Workspaces);
            
            var workspaces = workspaceService.GetAll();
            if (supervisor.Workspaces.Count == 0 || workspaces.Length == 0)
                throw new NooneWorkspaceFoundException();
            if (workspaces.All(w => w.Name != principal.CurrentUserIdentity.Workspace))
                throw new ActiveWorkspaceRemovedException();
            if (supervisor.Workspaces.All(w => w.Name != principal.CurrentUserIdentity.Workspace))
                throw new WorkspaceAccessException();
        }
        
        protected override Task ChangeWorkspaceAndNavigateToItAsync()
        {
            var workspaceView = workspaceService.GetAll().First();
            var supervisorIdentity = (SupervisorIdentity)principal.CurrentUserIdentity;
            supervisorIdentity.Workspace = workspaceView.Name;
            supervisorPlainStorage.Store(supervisorIdentity);
            
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

        protected override void UpdatePasswordOfResponsible(RestCredentials credentials)
        {
            var localSupervisor = this.supervisorsPlainStorage.FirstOrDefault();
            localSupervisor.PasswordHash = this.passwordHasher.Hash(credentials.Password);
            localSupervisor.Token = credentials.Token;

            this.supervisorsPlainStorage.Store(localSupervisor);
            this.principal.SignIn(localSupervisor.Name, credentials.Password, true);
        }

        protected override string GetRequiredUpdate(string targetVersion, string appVersion)
        {
            return EnumeratorUIResources.UpgradeRequired 
                   + Environment.NewLine + string.Format(EnumeratorUIResources.HeadquartersVersion, targetVersion) 
                   + Environment.NewLine + string.Format(EnumeratorUIResources.SupervisorVersion, appVersion);

        }
    }
}
