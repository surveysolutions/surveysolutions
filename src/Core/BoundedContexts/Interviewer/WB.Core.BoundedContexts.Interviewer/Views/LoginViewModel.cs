using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class LoginViewModel : BaseViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IPrincipal principal;
        private readonly IPasswordHasher passwordHasher;

        public LoginViewModel(IViewModelNavigationService viewModelNavigationService, IPrincipal principal, IPasswordHasher passwordHasher)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.passwordHasher = passwordHasher;
        }

        public string Login { get; private set; }
        public string Password { get; private set; }

        private bool areCredentialsWrong = false;
        public bool AreCredentialsWrong
        {
            get { return this.areCredentialsWrong; }
            set { this.areCredentialsWrong = value; this.RaisePropertyChanged(); }
        }

        public bool ShouldActivationButtonBeVisible = true;
        
        public void Init()
        {
#if DEBUG
            this.Login = "in1sv1";
            this.Password = "1234";
#endif
        }

        public IMvxCommand LoginCommand
        {
            get { return new MvxCommand(this.StartLogin); }
        }

        public IMvxCommand NavigateToSettingsCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateTo<SettingsViewModel>()); }
        }

        private void StartLogin()
        {
            try
            {
                this.principal.SignIn(this.Login, this.passwordHasher.Hash(this.Password), staySignedIn: true/*always true in current implementation*/);
                this.viewModelNavigationService.NavigateToDashboard();
            }
            catch (UnauthorizedAccessException)
            {

                this.AreCredentialsWrong = true;  
            }
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}