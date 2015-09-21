using System;
using System.Linq;
using System.Threading;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
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
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IRemoteAuthorizationService remoteAuthorizationService;
        private readonly ILogger logger;
        private readonly IPrincipal principal;

        private readonly IPasswordHasher passwordHasher;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;

        public LoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage, 
            IRemoteAuthorizationService remoteAuthorizationService, ILogger logger)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.passwordHasher = passwordHasher;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.remoteAuthorizationService = remoteAuthorizationService;
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
            get { return new MvxCommand(this.SignIn); }
        }

        public IMvxCommand OnlineSignInCommand
        {
            get { return new MvxCommand(this.OnlineSignIn); }
        }

        public IMvxCommand NavigateToTroubleshootingPageCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateTo<TroubleshootingViewModel>()); }
        }

        public void Init()
        {
            InterviewerIdentity currentInterviewer =
                this.interviewersPlainStorage.Query(interviewers => interviewers.FirstOrDefault());

            if (currentInterviewer == null)
            {
                this.viewModelNavigationService.NavigateTo<FinishInstallationViewModel>();
                return;
            }

            this.IsUserValid = true;
            this.UserName = currentInterviewer.Name;
        }

        private void SignIn()
        {
            this.ErrorMessage = string.Empty;
            var userName = this.UserName;
            var hashedPassword = this.passwordHasher.Hash(this.Password);

            this.IsUserValid = this.principal.SignIn(userName, hashedPassword, true);

            if (this.IsUserValid)
            {
                this.ResetCountOfFailedLoginAttempts();
                this.viewModelNavigationService.NavigateToDashboard();
            }
            else
            {
                this.IncreaseCountOfFailedLoginAttempts();
            }
        }

        private async void OnlineSignIn()
        {
            this.IsUserValid = true;
            this.ErrorMessage = string.Empty;

            var restCredentials = new RestCredentials()
            {
                Login = this.UserName,
                Password = this.passwordHasher.Hash(this.Password)
            };

            InterviewerApiView currentInterviewer;
            this.IsInProgress = true;
            try
            {
                currentInterviewer = await
                    this.remoteAuthorizationService.GetInterviewerAsync(restCredentials);
            }
            catch (SynchronizationException ex)
            {
                switch (ex.Type)
                {
                    case SynchronizationExceptionType.Unauthorized:
                        this.ErrorMessage = UIResources.Login_Online_SignIn_Failed;
                        break;
                    default:
                        this.ErrorMessage = ex.Message;
                        break;
                }
                return;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = InterviewerUIResources.UnexpectedException;
                this.logger.Error("Login view model. Unexpected exception", ex);
                return;
            }
            finally
            {
                this.IsInProgress = false;
            }

            var interviewerIdentity = new InterviewerIdentity
            {
                Id = currentInterviewer.Id.FormatGuid(),
                UserId = currentInterviewer.Id,
                SupervisorId = currentInterviewer.SupervisorId,
                Name = restCredentials.Login,
                Password = restCredentials.Password
            };

            await this.interviewersPlainStorage.RemoveAsync(interviewerIdentity.Id);
            await this.interviewersPlainStorage.StoreAsync(interviewerIdentity);

            this.IsUserValid = principal.SignIn(restCredentials.Login, restCredentials.Password, true);

            if (this.IsUserValid)
            {
                this.ResetCountOfFailedLoginAttempts();
                this.viewModelNavigationService.NavigateToDashboard();
            }
        }

        private void ResetCountOfFailedLoginAttempts()
        {
            this.countOfFailedLoginAttempts = 0;
            IsOnlineLoginButtonVisible=false;
        }

        private void IncreaseCountOfFailedLoginAttempts()
        {
            this.countOfFailedLoginAttempts++;
            IsOnlineLoginButtonVisible = countOfFailedLoginAttempts > 4;
        }
    }
}