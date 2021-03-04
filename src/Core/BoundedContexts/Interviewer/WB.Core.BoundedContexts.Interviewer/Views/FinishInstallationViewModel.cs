using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class FinishInstallationViewModel : EnumeratorFinishInstallationViewModel
    {
        private readonly IPasswordHasher passwordHasher;
        private readonly IInterviewerPrincipal interviewerPrincipal;
        private readonly IInterviewerSynchronizationService synchronizationService;
        
        public FinishInstallationViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IInterviewerPrincipal principal,
            IPasswordHasher passwordHasher,
            IDeviceSettings deviceSettings,
            IInterviewerSynchronizationService synchronizationService,
            ILogger logger,
            IQRBarcodeScanService qrBarcodeScanService,
            ISerializer serializer,
            IUserInteractionService userInteractionService,
            IAuditLogService auditLogService,
            IDeviceInformationService deviceInformationService) 
            : base(viewModelNavigationService, principal, deviceSettings, synchronizationService, 
                logger, qrBarcodeScanService, serializer, userInteractionService, auditLogService,
                deviceInformationService)
        {
            this.passwordHasher = passwordHasher;
            this.interviewerPrincipal = principal;
            this.synchronizationService = synchronizationService;
        }

        protected override string GetRequiredUpdateMessage(string targetVersion, string appVersion)
        {
            return EnumeratorUIResources.UpgradeRequired 
                   + Environment.NewLine + string.Format(EnumeratorUIResources.HeadquartersVersion, targetVersion) 
                   + Environment.NewLine + string.Format(EnumeratorUIResources.InterviewerVersion, appVersion);
        }

        protected override async Task RelinkUserToAnotherDeviceAsync(RestCredentials credentials, CancellationToken token)
        {
            var identity = await GenerateInterviewerIdentity(credentials, token);

            await this.ViewModelNavigationService
                .NavigateToAsync<RelinkDeviceViewModel, RelinkDeviceViewModelArg>(
                    new RelinkDeviceViewModelArg { Identity = identity });
        }

        protected override async Task SaveUserToLocalStorageAsync(RestCredentials credentials, CancellationToken token)
        {
            var interviewerIdentity = await GenerateInterviewerIdentity(credentials, token);

            this.interviewerPrincipal.SaveInterviewer(interviewerIdentity);
        }

        protected override async Task<List<WorkspaceApiView>> GetUserWorkspaces(RestCredentials credentials,
            CancellationToken token)
        {
            var interviewer = await this.synchronizationService.GetInterviewerAsync(credentials, token: token)
                .ConfigureAwait(false);
            return interviewer.Workspaces;
        }

        private async Task<InterviewerIdentity> GenerateInterviewerIdentity(RestCredentials credentials, CancellationToken token)
        {
            var interviewer = await this.synchronizationService.GetInterviewerAsync(credentials, token: token)
                .ConfigureAwait(false);
            var tenantId = await this.synchronizationService.GetTenantId(credentials, token).ConfigureAwait(false);

            var interviewerIdentity = new InterviewerIdentity
            {
                Id = interviewer.Id.FormatGuid(),
                UserId = interviewer.Id,
                SupervisorId = interviewer.SupervisorId,
                Name = this.UserName,
                PasswordHash = this.passwordHasher.Hash(this.Password),
                Token = credentials.Token,
                SecurityStamp = interviewer.SecurityStamp,
                TenantId = tenantId,
                Workspace = interviewer.Workspaces.First().Name,
            };
            return interviewerIdentity;
        }
    }
}
