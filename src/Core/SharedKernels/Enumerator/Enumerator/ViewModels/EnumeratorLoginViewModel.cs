using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class EnumeratorLoginViewModel : BaseViewModel
    {
        private readonly ILogger logger;
        private readonly IAuditLogService auditLogService;
        private readonly IPasswordHasher passwordHasher;
        private readonly ICompanyLogoStorage logoStorage;
        private readonly ISynchronizationService synchronizationService;

        protected EnumeratorLoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            ICompanyLogoStorage logoStorage,
            ISynchronizationService synchronizationService,
            ILogger logger,
            IAuditLogService auditLogService)
            : base(principal, viewModelNavigationService, false)
        {
            this.passwordHasher = passwordHasher;
            this.logoStorage = logoStorage;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.auditLogService = auditLogService;
        }

        public string UserName { get; set; }

        private string password;
        public string Password
        {
            get => this.password;
            set
            {
                SetProperty(ref this.password, value);
                this.PasswordError = null;
            }
        }

        private bool isUserValid;
        public bool IsUserValid
        {
            get => this.isUserValid;
            set => SetProperty(ref this.isUserValid, value);
        }

        private int countOfFailedLoginAttempts;

        private bool isOnlineLoginButtonVisible;
        public bool IsOnlineLoginButtonVisible
        {
            get => this.isOnlineLoginButtonVisible;
            set => SetProperty(ref this.isOnlineLoginButtonVisible, value);
        }  
        
        private string errorMessage;
        public string ErrorMessage
        {
            get => this.errorMessage;
            set => SetProperty(ref this.errorMessage, value);
        }

        public string PasswordError
        {
            get => passwordError;
            set => SetProperty(ref passwordError, value);
        }

        private bool isInProgress;
        private string passwordError;

        public bool IsInProgress
        {
            get => this.isInProgress;
            set => SetProperty(ref this.isInProgress, value);
        }

        public IMvxAsyncCommand SignInCommand => new MvxAsyncCommand(this.SignIn);
        public IMvxAsyncCommand OnlineSignInCommand => new MvxAsyncCommand(this.RemoteSignInAsync);
        public IMvxAsyncCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.ViewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>);

        public abstract bool HasUser();
        public abstract string GetUserName();
        public abstract string GetUserLastWorkspace();
        public abstract void UpdateLocalUser(string userName, string token, string passwordHash);

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);

            if (!this.HasUser()) return;

            var companyLogo = this.logoStorage.GetCompanyLogoByWorkspace(CompanyLogo.StorageKey, GetUserLastWorkspace());
            this.CustomLogo = companyLogo?.File;
            this.IsUserValid = true;
            this.UserName = this.GetUserName();
        }

        public override async void ViewCreated()
        {
            if (this.HasUser()) return;

            await this.ViewModelNavigationService.NavigateToFinishInstallationAsync();
        }

        public byte[] CustomLogo { get; private set; }

        private async Task SignIn()
        {
            var userName = this.UserName;

            this.logger.Trace($"Logging in {userName}");
            this.IsUserValid = this.Principal.SignIn(userName, this.Password, true);

            if (!this.IsUserValid)
            {
                this.PasswordError = EnumeratorUIResources.Login_WrongPassword;
                this.IncreaseCountOfFailedLoginAttempts();
                return;
            }
            else
            {
                auditLogService.WriteApplicationLevelRecord(new LoginAuditLogEntity(userName));
            }

            this.Password = string.Empty;
            await this.ViewModelNavigationService.NavigateToDashboardAsync();
            await this.ViewModelNavigationService.Close(this);
        }

        private async Task RemoteSignInAsync()
        {
            this.IsUserValid = true;
            
            var restCredentials = new RestCredentials {Login = this.UserName};
            this.IsInProgress = true;
            this.ErrorMessage = null;
            this.PasswordError = null;
            
            try
            {
                var token = await this.synchronizationService.LoginAsync(new LogonInfo
                {
                    Username = this.UserName,
                    Password = this.Password
                }, restCredentials);
                this.logger.Trace("Received token from remote signin");

                restCredentials.Token = token;

                var passwordHash = this.passwordHasher.Hash(this.Password);
                this.UpdateLocalUser(UserName, token, passwordHash);

                await this.SignIn();
            }
            catch (SynchronizationException ex)
            {
                switch (ex.Type)
                {
                    case SynchronizationExceptionType.Unauthorized:
                        this.ErrorMessage = EnumeratorUIResources.Login_Online_SignIn_Failed;
                        break;
                    default:
                        this.ErrorMessage = ex.Message;
                        break;
                }
                this.IsUserValid = false;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = EnumeratorUIResources.UnexpectedException;
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
                this.ErrorMessage = EnumeratorUIResources.Login_Online_Signin_Explanation_message;
        }
    }
}
