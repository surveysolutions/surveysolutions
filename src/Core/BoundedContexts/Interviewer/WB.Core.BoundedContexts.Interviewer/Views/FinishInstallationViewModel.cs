using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly IQRBarcodeScanService qrBarcodeScanService;

        public FinishInstallationViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IDeviceSettings deviceSettings,
            IInterviewerSynchronizationService synchronizationService,
            ILogger logger,
            IQRBarcodeScanService qrBarcodeScanService,
            IUserInteractionService userInteractionService) : base(viewModelNavigationService, principal, deviceSettings, synchronizationService, logger, userInteractionService)
        {
            this.passwordHasher = passwordHasher;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.synchronizationService = synchronizationService;
            this.qrBarcodeScanService = qrBarcodeScanService;
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
                Token = credentials.Token
            };

            this.interviewersPlainStorage.Store(interviewerIdentity);
        }

        private IMvxAsyncCommand scanAsyncCommand;
        public IMvxAsyncCommand ScanCommand
        {
            get { return this.scanAsyncCommand ?? (this.scanAsyncCommand = new MvxAsyncCommand(this.ScanAsync, () => !IsInProgress)); }
        }

        private async Task ScanAsync()
        {
            this.IsInProgress = true;

            try
            {
                var scanCode = await this.qrBarcodeScanService.ScanAsync();

                if (scanCode != null)
                {
                    if (Uri.TryCreate(scanCode.Code, UriKind.Absolute, out var uriResult))
                    {
                        var seachTerm = "/api/interviewersync";
                        var position = scanCode.Code.IndexOf(seachTerm, StringComparison.InvariantCultureIgnoreCase);

                        this.Endpoint = position > 0 ? scanCode.Code.Substring(0, position) : scanCode.Code;
                    }
                    else
                    {
                        var finishInfo = JsonConvert.DeserializeObject<FinishInstallationInfo>(scanCode.Code);

                        this.Endpoint = finishInfo.Url;
                        this.UserName = finishInfo.Login;
                        this.Password = finishInfo.Password;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                this.IsInProgress = false;
            }
        }
    }
}
