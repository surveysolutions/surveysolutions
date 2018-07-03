using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class EnumeratorLoginViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ILogger logger;
        private readonly IAuditLogService auditLogService;
        private readonly IPasswordHasher passwordHasher;
        private readonly IPlainStorage<CompanyLogo> logoStorage;
        private readonly ISynchronizationService synchronizationService;

        protected EnumeratorLoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<CompanyLogo> logoStorage,
            ISynchronizationService synchronizationService,
            ILogger logger,
            IAuditLogService auditLogService)
            : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.passwordHasher = passwordHasher;
            this.logoStorage = logoStorage;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.auditLogService = auditLogService;
        }

        public override bool IsAuthenticationRequired => false;

        public string UserName { get; set; }

        private string password;
        public string Password
        {
            get => this.password;
            set => SetProperty(ref this.password, value);
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

        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set => SetProperty(ref this.isInProgress, value);
        }

        public IMvxAsyncCommand SignInCommand => new MvxAsyncCommand(this.SignIn);
        public IMvxAsyncCommand OnlineSignInCommand => new MvxAsyncCommand(this.RemoteSignInAsync);
        public IMvxAsyncCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>);

        public abstract bool HasUser();
        public abstract string GetUserName();
        public abstract void UpdateLocalUser(string token, string passwordHash);

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);

            if (!this.HasUser()) return;

            var companyLogo = this.logoStorage.GetById(CompanyLogo.StorageKey);
            this.CustomLogo = companyLogo?.File;
            this.IsUserValid = true;
            this.UserName = this.GetUserName();
            this.ErrorMessage = InterviewerUIResources.Login_WrongPassword;
        }

        public override async void ViewCreated()
        {
            if (this.HasUser()) return;

            await this.viewModelNavigationService.NavigateToFinishInstallationAsync();
        }

        public byte[] CustomLogo { get; private set; }

        private async Task SignIn()
        {
            var userName = this.UserName;

            this.IsUserValid = this.principal.SignIn(userName, this.Password, true);

            if (!this.IsUserValid)
            {
                this.IncreaseCountOfFailedLoginAttempts();
                return;
            }
            else
            {
                auditLogService.Write(new LoginAuditLogEntity(userName));
            }

            await this.viewModelNavigationService.NavigateToDashboardAsync();
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

                this.UpdateLocalUser(token, this.passwordHasher.Hash(this.Password));

                await this.SignIn();
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
