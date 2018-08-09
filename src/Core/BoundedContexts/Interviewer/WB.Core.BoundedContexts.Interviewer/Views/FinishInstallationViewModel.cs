using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class FinishInstallationViewModel : EnumeratorFinishInstallationViewModel
    {
        private readonly IPasswordHasher passwordHasher;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IInterviewerSynchronizationService synchronizationService;
        
        public FinishInstallationViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IDeviceSettings deviceSettings,
            IInterviewerSynchronizationService synchronizationService,
            ILogger logger,
            IQRBarcodeScanService qrBarcodeScanService,
            IUserInteractionService userInteractionService) 
            : base(viewModelNavigationService, principal, deviceSettings, synchronizationService, logger, qrBarcodeScanService, userInteractionService)
        {
            this.passwordHasher = passwordHasher;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.synchronizationService = synchronizationService;
        }

        protected override string GetAppPrefixUrl()
        {
            return synchronizationService.ApiDownloadAppPrefixUrl;
        }

        protected override async Task RelinkUserToAnotherDeviceAsync(RestCredentials credentials, CancellationToken token)
        {
            await this.SaveUserToLocalStorageAsync(credentials, token);
            var interviewerIdentity = this.interviewersPlainStorage.FirstOrDefault();

            await this.viewModelNavigationService
                .NavigateToAsync<RelinkDeviceViewModel, RelinkDeviceViewModelArg>(
                    new RelinkDeviceViewModelArg { Identity = interviewerIdentity });
        }

        protected override async Task SaveUserToLocalStorageAsync(RestCredentials credentials, CancellationToken token)
        {
            var interviewer = await this.synchronizationService.GetInterviewerAsync(credentials, token: token).ConfigureAwait(false);

            var interviewerIdentity = new InterviewerIdentity
            {
                Id = interviewer.Id.FormatGuid(),
                UserId = interviewer.Id,
                SupervisorId = interviewer.SupervisorId,
                Name = this.UserName,
                PasswordHash = this.passwordHasher.Hash(this.Password),
                Token = credentials.Token,
                SecurityStamp = interviewer.SecurityStamp
            };

            this.interviewersPlainStorage.Store(interviewerIdentity);
        }
    }
}
