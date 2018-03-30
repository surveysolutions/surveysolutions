using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class FinishInstallationViewModel : BaseViewModel<FinishInstallationViewModelArg>
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IPasswordHasher passwordHasher;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;
        private CancellationTokenSource cancellationTokenSource;
        private readonly IUserInteractionService userInteractionService;

        public FinishInstallationViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IInterviewerSettings interviewerSettings,
            ISynchronizationService synchronizationService,
            ILogger logger, 
            IUserInteractionService userInteractionService) : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.passwordHasher = passwordHasher;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.interviewerSettings = interviewerSettings;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.userInteractionService = userInteractionService;
        }

        protected override bool IsAuthenticationRequired => false;

        private string endpoint;
        public string Endpoint
        {
            get { return this.endpoint; }
            set { this.endpoint = value; RaisePropertyChanged(); }
        }

        private string userName;
        public string UserName
        {
            get { return this.userName; }
            set { this.userName = value; RaisePropertyChanged(); }
        }

        private string password;
        public string Password
        {
            get { return this.password; }
            set { this.password = value; RaisePropertyChanged(); }
        }

        private bool isEndpointValid;
        public bool IsEndpointValid
        {
            get { return this.isEndpointValid; }
            set { this.isEndpointValid = value; RaisePropertyChanged(); }
        }

        private bool isUserValid;
        public bool IsUserValid
        {
            get { return this.isUserValid; }
            set { this.isUserValid = value; RaisePropertyChanged(); }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set { this.errorMessage = value; RaisePropertyChanged(); }
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; RaisePropertyChanged(); }
        }

        private IMvxAsyncCommand signInCommand;
        public IMvxAsyncCommand SignInCommand
        {
            get { return this.signInCommand ?? (this.signInCommand = new MvxAsyncCommand(this.SignInAsync, () => !IsInProgress)); }
        }

        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>);

        private InterviewerIdentity userIdentity;

        public override void Prepare(FinishInstallationViewModelArg parameter)
        {
            this.userIdentity = parameter.UserIdentity;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            this.IsUserValid = true;
            this.IsEndpointValid = true;
            this.Endpoint =  this.interviewerSettings.Endpoint;
            this.UserName = this.userIdentity.Name;

#if DEBUG
            this.Endpoint = "http://192.168.88./headquarters";
            this.UserName = "int";
            this.Password = "1";
#endif
        }

        public async Task RefreshEndpoint()
        {
            var settingsEndpoint = this.interviewerSettings.Endpoint;
            if (!string.IsNullOrEmpty(settingsEndpoint) && !string.Equals(settingsEndpoint, this.endpoint, StringComparison.OrdinalIgnoreCase))
            {
                var message = string.Format(InterviewerUIResources.FinishInstallation_EndpointDiffers,  this.Endpoint, settingsEndpoint);
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
            bool isNeedNavigateToRelinkPage = false;

            if (this.Endpoint?.StartsWith("@") == true)
            {
                this.Endpoint = $"https://{this.Endpoint.Substring(1)}.mysurvey.solutions";
            }

            this.interviewerSettings.SetEndpoint(this.Endpoint);

            var restCredentials = new RestCredentials
            {
                Login = this.userName
            };

            cancellationTokenSource = new CancellationTokenSource();
            this.IsInProgress = true;
            InterviewerIdentity interviewerIdentity = null;
            try
            {
                if (string.IsNullOrWhiteSpace(UserName))
                {
                    throw new SynchronizationException(SynchronizationExceptionType.Unauthorized, InterviewerUIResources.Login_WrongPassword);
                }

                var authToken = await this.synchronizationService.LoginAsync(new LogonInfo
                {
                    Username = this.UserName,
                    Password = this.Password
                }, restCredentials, cancellationTokenSource.Token).ConfigureAwait(false);

                restCredentials.Token = authToken;

                var interviewer = await this.synchronizationService.GetInterviewerAsync(restCredentials, token: cancellationTokenSource.Token).ConfigureAwait(false);

                interviewerIdentity = new InterviewerIdentity
                {
                    Id = interviewer.Id.FormatGuid(),
                    UserId = interviewer.Id,
                    SupervisorId = interviewer.SupervisorId,
                    Name = this.UserName,
                    PasswordHash = this.passwordHasher.Hash(this.Password),
                    Token = restCredentials.Token
                };

                if (!await this.synchronizationService.HasCurrentInterviewerDeviceAsync(credentials: restCredentials, token: cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    await this.synchronizationService.LinkCurrentInterviewerToDeviceAsync(credentials: restCredentials, token: cancellationTokenSource.Token).ConfigureAwait(false);
                }

                await this.synchronizationService.CanSynchronizeAsync(credentials: restCredentials, token: cancellationTokenSource.Token).ConfigureAwait(false);
                
                this.interviewersPlainStorage.Store(interviewerIdentity);

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
                        isNeedNavigateToRelinkPage = true;
                        break;
                }
                this.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = InterviewerUIResources.UnexpectedException;
                this.logger.Error("Login view model. Unexpected exception", ex);
            }
            finally
            {
                this.IsInProgress = false;
                cancellationTokenSource = null;
            }

            if (isNeedNavigateToRelinkPage)
                await this.viewModelNavigationService.NavigateToAsync<RelinkDeviceViewModel, RelinkDeviceViewModelArg>(new RelinkDeviceViewModelArg{ Identity = interviewerIdentity});
        }

        public void CancellInProgressTask()
        {
            this.cancellationTokenSource?.Cancel();
        }
    }
}
