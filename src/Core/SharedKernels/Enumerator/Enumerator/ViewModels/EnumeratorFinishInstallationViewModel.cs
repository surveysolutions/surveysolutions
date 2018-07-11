using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class EnumeratorFinishInstallationViewModel : BaseViewModel<FinishInstallationViewModelArg>
    {
        protected readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IDeviceSettings deviceSettings;
        private readonly IRemoteAuthorizationService synchronizationService;
        private readonly ILogger logger;
        private CancellationTokenSource cancellationTokenSource;
        private readonly IUserInteractionService userInteractionService;
        private const string StateKey = "identity";

        protected EnumeratorFinishInstallationViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IDeviceSettings deviceSettings,
            IRemoteAuthorizationService synchronizationService,
            ILogger logger,
            IUserInteractionService userInteractionService) : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.deviceSettings = deviceSettings;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.userInteractionService = userInteractionService;
        }

        protected override bool IsAuthenticationRequired => false;

        private string endpoint;

        public string Endpoint
        {
            get => this.endpoint;
            set
            {
                this.endpoint = value;
                RaisePropertyChanged();
            }
        }

        private string userName;

        public string UserName
        {
            get => this.userName;
            set
            {
                this.userName = value;
                RaisePropertyChanged();
            }
        }

        private string password;

        public string Password
        {
            get => this.password;
            set
            {
                this.password = value;
                RaisePropertyChanged();
            }
        }

        private bool isEndpointValid;

        public bool IsEndpointValid
        {
            get => this.isEndpointValid;
            set
            {
                this.isEndpointValid = value;
                RaisePropertyChanged();
            }
        }

        private bool isUserValid;

        public bool IsUserValid
        {
            get => this.isUserValid;
            set
            {
                this.isUserValid = value;
                RaisePropertyChanged();
            }
        }

        private string errorMessage;

        public string ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                this.errorMessage = value;
                RaisePropertyChanged();
            }
        }

        private bool isInProgress;

        public bool IsInProgress
        {
            get => this.isInProgress;
            set
            {
                this.isInProgress = value;
                RaisePropertyChanged();
            }
        }

        private IMvxAsyncCommand signInCommand;

        public IMvxAsyncCommand SignInCommand
        {
            get
            {
                return this.signInCommand ??
                       (this.signInCommand = new MvxAsyncCommand(this.SignInAsync, () => !IsInProgress));
            }
        }

        public IMvxAsyncCommand NavigateToDiagnosticsPageCommand =>
            new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>);

        public override void Prepare(FinishInstallationViewModelArg parameter)
        {
            this.UserName = parameter.UserName;
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);

            this.IsUserValid = true;
            this.IsEndpointValid = true;
            this.Endpoint = this.deviceSettings.Endpoint;

#if DEBUG
            this.Endpoint = "http://192.168.88./headquarters";
            this.UserName = "int";
            this.Password = "1";
#endif
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            if (this.UserName != null)
            {
                bundle.Data[StateKey] = this.userName;
            }
        }

        protected override void ReloadFromBundle(IMvxBundle state)
        {
            base.ReloadFromBundle(state);
            if (state.Data.ContainsKey(StateKey))
            {
                this.UserName = state.Data[StateKey];
            }
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

#pragma warning disable 4014
            RefreshEndpoint(); // It is handing if awaited
#pragma warning restore 4014
        }

        public async Task RefreshEndpoint()
        {
            var settingsEndpoint = this.deviceSettings.Endpoint;
            if (!string.IsNullOrEmpty(settingsEndpoint) &&
                !string.Equals(settingsEndpoint, this.endpoint, StringComparison.OrdinalIgnoreCase))
            {
                var message = string.Format(InterviewerUIResources.FinishInstallation_EndpointDiffers, this.Endpoint,
                    settingsEndpoint);
                if (await this.userInteractionService.ConfirmAsync(message, isHtml: false).ConfigureAwait(false))
                {
                    this.Endpoint = settingsEndpoint;
                }
            }
        }

        private async Task SignInAsync()
        {
            this.IsUserValid = true;
            this.IsEndpointValid = true;

            if (this.Endpoint?.StartsWith("@") == true)
            {
                this.Endpoint = $"https://{this.Endpoint.Substring(1)}.mysurvey.solutions";
            }

            this.deviceSettings.SetEndpoint(this.Endpoint);

            var restCredentials = new RestCredentials
            {
                Login = this.userName
            };

            cancellationTokenSource = new CancellationTokenSource();
            this.IsInProgress = true;
            try
            {
                if (string.IsNullOrWhiteSpace(UserName))
                {
                    throw new SynchronizationException(SynchronizationExceptionType.Unauthorized,
                        InterviewerUIResources.Login_WrongPassword);
                }

                var authToken = await this.synchronizationService.LoginAsync(new LogonInfo
                {
                    Username = this.UserName,
                    Password = this.Password
                }, restCredentials, cancellationTokenSource.Token).ConfigureAwait(false);

                restCredentials.Token = authToken;

                if (!await this.synchronizationService
                    .HasCurrentUserDeviceAsync(credentials: restCredentials, token: cancellationTokenSource.Token)
                    .ConfigureAwait(false))
                {
                    await this.synchronizationService
                        .LinkCurrentUserToDeviceAsync(credentials: restCredentials,
                            token: cancellationTokenSource.Token).ConfigureAwait(false);
                }

                await this.synchronizationService
                    .CanSynchronizeAsync(credentials: restCredentials, token: cancellationTokenSource.Token)
                    .ConfigureAwait(false);

                await this.SaveUserToLocalStorageAsync(restCredentials, cancellationTokenSource.Token);

                this.Principal.SignIn(restCredentials.Login, this.Password, true);
                await this.viewModelNavigationService.NavigateToDashboardAsync();
            }
            catch (SynchronizationException ex)
            {
                switch (ex.Type)
                {
                    case SynchronizationExceptionType.HostUnreachable:
                    case SynchronizationExceptionType.InvalidUrl:
                    case SynchronizationExceptionType.ServiceUnavailable:
                        this.IsEndpointValid = false;
                        break;
                    case SynchronizationExceptionType.UserIsNotInterviewer:
                    case SynchronizationExceptionType.UserLocked:
                    case SynchronizationExceptionType.UserNotApproved:
                    case SynchronizationExceptionType.Unauthorized:
                        this.IsUserValid = false;
                        break;
                    case SynchronizationExceptionType.UserLinkedToAnotherDevice:
                        await this.RelinkUserToAnotherDeviceAsync(restCredentials, cancellationTokenSource.Token);
                        break;
                }

                this.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = InterviewerUIResources.UnexpectedException;
                this.logger.Error("Finish installation view model. Unexpected exception", ex);
            }
            finally
            {
                this.IsInProgress = false;
                cancellationTokenSource = null;
            }
        }

        protected abstract Task RelinkUserToAnotherDeviceAsync(RestCredentials credentials, CancellationToken token);
        protected abstract Task SaveUserToLocalStorageAsync(RestCredentials credentials, CancellationToken token);

        public void CancellInProgressTask() => this.cancellationTokenSource?.Cancel();
    }
}
