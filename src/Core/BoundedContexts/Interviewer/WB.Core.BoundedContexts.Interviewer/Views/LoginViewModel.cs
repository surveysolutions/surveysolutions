using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ILogger logger;
        private readonly IPasswordHasher passwordHasher;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IPlainStorage<CompanyLogo> logoStorage;
        private readonly ISynchronizationService synchronizationService;

        public LoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IPlainStorage<CompanyLogo> logoStorage,
            ISynchronizationService synchronizationService,
            ILogger logger)
            : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.passwordHasher = passwordHasher;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.logoStorage = logoStorage;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
        }

        public override bool IsAuthenticationRequired => false;

        public string UserName { get; set; }

        private string password;
        public string Password
        {
            get { return this.password; }
            set { this.password = value; RaisePropertyChanged(); }
        }

        private bool isUserValid;
        public bool IsUserValid
        {
            get { return this.isUserValid; }
            set { this.isUserValid = value; RaisePropertyChanged(); }
        }

        private int countOfFailedLoginAttempts;

        private bool isOnlineLoginButtonVisible;
        public bool IsOnlineLoginButtonVisible

        {
            get { return this.isOnlineLoginButtonVisible; }
            set { this.isOnlineLoginButtonVisible = value; RaisePropertyChanged(); }
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

        public IMvxCommand SignInCommand => new MvxCommand(this.SignIn);

        public IMvxAsyncCommand OnlineSignInCommand => new MvxAsyncCommand(this.RemoteSignInAsync);

        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxCommand(() => this.viewModelNavigationService.NavigateTo<DiagnosticsViewModel>());

        public override void Load()
        {
            InterviewerIdentity currentInterviewer = this.interviewersPlainStorage.FirstOrDefault();

            if (currentInterviewer == null)
            {
                this.viewModelNavigationService.NavigateTo<FinishInstallationViewModel>();
                return;
            }

            var companyLogo = this.logoStorage.GetById(CompanyLogo.StorageKey);
            this.CustomLogo = companyLogo?.File;
            this.IsUserValid = true;
            this.UserName = currentInterviewer.Name;
            this.ErrorMessage = InterviewerUIResources.Login_WrongPassword;

            ServiceLocator.Current.GetInstance<InterviewerDashboardEventHandler>(); // Needed to create instance of it
        }

        public byte[] CustomLogo { get; private set; }

        private void SignIn()
        {
            var userName = this.UserName;

            this.IsUserValid = this.principal.SignIn(userName, this.Password, true);

            if (!this.IsUserValid)
            {
                this.IncreaseCountOfFailedLoginAttempts();
                return;
            }

            this.viewModelNavigationService.NavigateToDashboard();
        }

        private async Task RemoteSignInAsync()
        {
            this.IsUserValid = true;

            var restCredentials = new RestCredentials {Login = this.UserName};
            this.IsInProgress = true;
            this.ErrorMessage = String.Empty;
            
            try
            {
                var token = await this.synchronizationService.LoginAsync(new LogonInfo
                {
                    Username = this.UserName,
                    Password = this.Password
                }, restCredentials);

                restCredentials.Token = token;

                await this.synchronizationService.GetInterviewerAsync(restCredentials);

                var localInterviewer = this.interviewersPlainStorage.FirstOrDefault();
                localInterviewer.Token = token;
                localInterviewer.PasswordHash = this.passwordHasher.Hash(this.Password);

                this.interviewersPlainStorage.Store(localInterviewer);

                this.SignIn();
            }
            catch (SynchronizationException ex)
            {
                switch (ex.Type)
                {
                    case SynchronizationExceptionType.Unauthorized:
                        this.ErrorMessage = InterviewerUIResources.Login_Online_SignIn_Failed;
                        break;
                    default:
                        this.ErrorMessage = ex.Message;
                        break;
                }
                this.IsUserValid = false;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = InterviewerUIResources.UnexpectedException;
                this.logger.Error("Login view model. Unexpected exception", ex);
                this.IsUserValid = false;
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        private void IncreaseCountOfFailedLoginAttempts()
        {
            this.countOfFailedLoginAttempts++;
            IsOnlineLoginButtonVisible = countOfFailedLoginAttempts > 4;
            if(this.IsOnlineLoginButtonVisible)
                this.ErrorMessage = InterviewerUIResources.Login_Online_Signin_Explanation_message;
        }
    }
}