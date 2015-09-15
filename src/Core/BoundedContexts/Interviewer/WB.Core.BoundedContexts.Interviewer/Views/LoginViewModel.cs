using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Cirrious.MvvmCross.Plugins.Network.Droid;
using Cirrious.MvvmCross.Plugins.Network.Reachability;
using Cirrious.MvvmCross.ViewModels;
using Ncqrs.Domain.Storage;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class LoginViewModel : BaseViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IPrincipal principal;
        private readonly IPasswordHasher passwordHasher;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly ISynchronizationService synchronizationService;
        private readonly IMvxReachability reachability;
        private readonly ILogger logger;

        public LoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IInterviewerSettings interviewerSettings,
            ISynchronizationService synchronizationService,
            IMvxReachability reachability, ILogger logger)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.passwordHasher = passwordHasher;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.interviewerSettings = interviewerSettings;
            this.synchronizationService = synchronizationService;
            this.reachability = reachability;
            this.logger = logger;
        }

        public string Endpoint
        {
            get { return this.endpoint; }
            set { this.endpoint = value; RaisePropertyChanged(); }
        }

        public string Login
        {
            get { return this.login; }
            set { this.login = value; RaisePropertyChanged(); }
        }

        public string Password
        {
            get { return this.password; }
            set { this.password = value; RaisePropertyChanged(); }
        }

        public bool IsEndpointValid
        {
            get { return this.isEndpointValid; }
            set { this.isEndpointValid = value; RaisePropertyChanged(); }
        }

        public bool IsLoginValid
        {
            get { return this.isLoginValid; }
            set { this.isLoginValid = value; RaisePropertyChanged(); }
        }

        public bool IsPasswordValid
        {
            get { return this.isPasswordValid; }
            set { this.isPasswordValid = value; RaisePropertyChanged(); }
        }

        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set { this.errorMessage = value; RaisePropertyChanged(); }
        }

        private bool areCredentialsWrong = false;
        public bool AreCredentialsWrong
        {
            get { return this.areCredentialsWrong; }
            set { this.areCredentialsWrong = value; this.RaisePropertyChanged(); }
        }

        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; RaisePropertyChanged(); }
        }

        public bool IsFinishInstallationMode { get; set; }

        public bool IsNewVersionAvailable { get; set; }

        public bool EndpointWasReached { get; set; }

        public void Init()
        {
#if DEBUG
            this.Endpoint = "http://192.168.88.163/headquarters";
            this.Login = "in1sv1";
            this.Password = "1234";
#endif
            IsNewVersionAvailable = false;
            IsLoginValid = true;
            IsEndpointValid = true;
            IsPasswordValid = true;
            
            InterviewerIdentity currentInterviewer = this.interviewersPlainStorage.Query(interviewers => interviewers.FirstOrDefault());

            if (currentInterviewer == null)
            {
                IsFinishInstallationMode = true;
            }
            else
            {
                Login = currentInterviewer.Name;

                EndpointWasReached = this.reachability.IsHostReachable(this.Endpoint);
            }
        }

        private bool isEndpointValid;

        private bool isLoginValid;

        private bool isPasswordValid;

        private IMvxCommand loginCommand;

        private bool isInProgress;

        private string password;

        private string login;

        private string endpoint;

        private string errorMessage;

        public IMvxCommand LoginCommand
        {
            get { return this.loginCommand ?? (this.loginCommand = new MvxCommand(async () => await this.LoginAsync(), () => !IsInProgress)); }
        }

        public IMvxCommand NavigateToSettingsCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateTo<SettingsViewModel>()); }
        }

        private async Task LoginAsync()
        {
            IsInProgress = true;

            var userName = this.Login;
            var hashedPassword = this.passwordHasher.Hash(this.Password);

            if (this.IsFinishInstallationMode)
            {
                var restCredentials = new RestCredentials { Login = userName, Password = hashedPassword };

                if (!this.reachability.IsHostReachable(this.Endpoint))
                {
                    ErrorMessage = UIResources.Login_Validation_EndpointIsUnreachable;
                    this.IsEndpointValid = false;
                    this.IsInProgress = false;
                    return;
                }

                this.IsEndpointValid = true;
                this.interviewerSettings.SetSyncAddressPoint(this.Endpoint);

                InterviewerApiView interviewer;
                try
                {
                    interviewer = await this.synchronizationService.GetCurrentInterviewerAsync(login: userName, password: hashedPassword, token: default(CancellationToken));

                    await this.interviewersPlainStorage.StoreAsync(
                        new InterviewerIdentity
                        {
                            Id = interviewer.Id.FormatGuid(),
                            UserId = interviewer.Id,
                            SupervisorId = interviewer.SupervisorId,
                            Name = userName,
                            Password = hashedPassword
                        });

                    this.LoginUserOffline(userName, hashedPassword);
                }
                catch (Exception exception)
                {
                    IsLoginValid = false;
                    IsPasswordValid = false;
                    ErrorMessage = exception.Message;
                    this.IsInProgress = false;
                    logger.Error(string.Format("Error occured while authorizing user {0} online.", userName), exception);
                    return;
                }

                bool interviewerHasLinkedDevice;
                try
                {
                    interviewerHasLinkedDevice = await this.synchronizationService.HasCurrentInterviewerDeviceAsync(token: default(CancellationToken), credentials: restCredentials);
                }
                catch (RestException exception)
                {
                    ErrorMessage = "Error. " + exception.Message;
                    this.IsInProgress = false;
                    logger.Error(string.Format("Error occured while checking user {0} device linking status.", userName), exception);
                    return;
                }

                if (!interviewerHasLinkedDevice)
                {
                    await this.synchronizationService.LinkCurrentInterviewerToDeviceAsync(token: default(CancellationToken), credentials: restCredentials);
                }
                else
                {
                    bool isAnotherDeviceLinkedToInterviewer;
                    try
                    {
                        isAnotherDeviceLinkedToInterviewer = await this.synchronizationService.IsDeviceLinkedToCurrentInterviewerAsync(token: default(CancellationToken), credentials: restCredentials);
                    }
                    catch (RestException exception)
                    {
                        ErrorMessage = "Error. " + exception.Message;
                        this.IsInProgress = false;
                        logger.Error(string.Format("Error occured while checking user {0} device.", userName), exception);
                        return;
                    }

                    if (!isAnotherDeviceLinkedToInterviewer)
                    {
                        this.viewModelNavigationService.NavigateTo<RelinkDeviceViewModel>(new { redirectedFromFinishInstallation = true });
                        this.IsInProgress = false;
                        return;
                    }
                }
            }
          
            this.LoginUserOfflineAndGoToDashboard(userName, hashedPassword);

            IsInProgress = false;
        }

        private void LoginUserOfflineAndGoToDashboard(string userName, string hashedPassword)
        {
            try
            {
                this.principal.SignIn(userName, hashedPassword, true);

                this.viewModelNavigationService.NavigateTo<DashboardViewModel>();
            }
            catch (UnauthorizedAccessException exception)
            {
                if (IsFinishInstallationMode)
                {
                    IsLoginValid = false;
                }
                IsPasswordValid = false;
                logger.Error(string.Format("Error occured while authorizing user {0} offline.", userName), exception);
                ErrorMessage = exception.Message;
            }
        }

        public override void NavigateToPreviousViewModel()
        {

        }
    }
}