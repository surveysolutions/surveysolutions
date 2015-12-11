using System;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
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
    public class LoginViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ILogger logger;
        private readonly IPrincipal principal;
        private readonly IPasswordHasher passwordHasher;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly ISynchronizationService synchronizationService;

        public LoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage, 
            ISynchronizationService synchronizationService,
            ILogger logger)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.passwordHasher = passwordHasher;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
        }

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

        public IMvxCommand SignInCommand
        {
            get { return new MvxCommand(async () => await this.SignInAsync()); }
        }

        public IMvxCommand OnlineSignInCommand
        {
            get { return new MvxCommand(async () => await this.RemoteSignInAsync()); }
        }

        public IMvxCommand NavigateToDiagnosticsPageCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>()); }
        }

        public IMvxCommand NavigateToTroubleshootingPageCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToAsync<TroubleshootingViewModel>()); }
        }

        public async void Init()
        {
            InterviewerIdentity currentInterviewer =
                this.interviewersPlainStorage.Query(interviewers => interviewers.FirstOrDefault());

            if (currentInterviewer == null)
            {
                await this.viewModelNavigationService.NavigateToAsync<FinishInstallationViewModel>();
                return;
            }

            this.IsUserValid = true;
            this.UserName = currentInterviewer.Name;
            this.ErrorMessage = InterviewerUIResources.Login_WrondPassword;
        }

        private async Task SignInAsync()
        {
            var userName = this.UserName;
            var hashedPassword = this.passwordHasher.Hash(this.Password);

            this.IsUserValid = this.principal.SignIn(userName, hashedPassword, true);

            if (!this.IsUserValid)
            {
                this.IncreaseCountOfFailedLoginAttempts();
                return;
            }

            await this.viewModelNavigationService.NavigateToDashboardAsync();
        }

        private async Task RemoteSignInAsync()
        {
            this.IsUserValid = true;

            var restCredentials = new RestCredentials
            {
                Login = this.UserName,
                Password = this.passwordHasher.Hash(this.Password)
            };

            this.IsInProgress = true;
            try
            {
                await this.synchronizationService.GetInterviewerAsync(restCredentials);

                var localInterviewer = this.interviewersPlainStorage.Query(interviewers => interviewers.FirstOrDefault());
                localInterviewer.Password = restCredentials.Password;

                await this.interviewersPlainStorage.StoreAsync(localInterviewer);

                await this.SignInAsync();
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