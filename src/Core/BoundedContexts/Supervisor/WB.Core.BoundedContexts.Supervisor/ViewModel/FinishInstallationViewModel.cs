using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class FinishInstallationViewModel : EnumeratorFinishInstallationViewModel
    {
        private readonly IPasswordHasher passwordHasher;
        private readonly IPlainStorage<SupervisorIdentity> supervisorsPlainStorage;
        private readonly ISupervisorSynchronizationService synchronizationService;


        public FinishInstallationViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<SupervisorIdentity> interviewersPlainStorage,
            IDeviceSettings deviceSettings,
            ISupervisorSynchronizationService synchronizationService,
            ILogger logger,
            IQRBarcodeScanService qrBarcodeScanService,
            ISerializer serializer,
            IUserInteractionService userInteractionService,
            IAuditLogService auditLogService,
            IDeviceInformationService deviceInformationService,
            IWorkspaceService workspaceService) 
            : base(viewModelNavigationService, principal, deviceSettings, synchronizationService, 
                logger, qrBarcodeScanService, serializer, userInteractionService, auditLogService,
                deviceInformationService, workspaceService)
        {
            this.passwordHasher = passwordHasher;
            this.supervisorsPlainStorage = interviewersPlainStorage;
            this.synchronizationService = synchronizationService;
        }
        
        public override async Task Initialize()
        {
            await base.Initialize();
#if DEBUG
            this.Endpoint = "http://192.168.88./headquarters";
            this.UserName = "sup";
            this.Password = "1";
#endif
        }

        protected override string GetRequiredUpdateMessage(string targetVersion, string appVersion)
        {
            return EnumeratorUIResources.UpgradeRequired 
                   + Environment.NewLine + string.Format(EnumeratorUIResources.HeadquartersVersion, targetVersion) 
                   + Environment.NewLine + string.Format(EnumeratorUIResources.SupervisorVersion, appVersion);
        }

        protected override Task RelinkUserToAnotherDeviceAsync(RestCredentials credentials, CancellationToken token) => throw new NotImplementedException();

        protected override async Task SaveUserToLocalStorageAsync(RestCredentials credentials, CancellationToken token)
        {
            var supervisor = await this.synchronizationService.GetSupervisorAsync(credentials, token: token).ConfigureAwait(false);
            var tenantId = await this.synchronizationService.GetTenantId(credentials, token).ConfigureAwait(false);

            var supervisorIdentity = new SupervisorIdentity
            {
                Id = supervisor.Id.FormatGuid(),
                UserId = supervisor.Id,
                Name = this.UserName,
                Email = supervisor.Email,
                PasswordHash = this.passwordHasher.Hash(this.Password),
                Token = credentials.Token,
                TenantId = tenantId,
                Workspace = supervisor.Workspaces.First().Name
            };

            this.supervisorsPlainStorage.Store(supervisorIdentity);
        }

        protected override async Task<List<WorkspaceApiView>> GetUserWorkspaces(RestCredentials credentials, CancellationToken token)
        {
            var supervisor = await this.synchronizationService.GetSupervisorAsync(credentials, token: token).ConfigureAwait(false);
            return supervisor.Workspaces;
        }
    }
}
