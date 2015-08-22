using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Interviewer.ViewModel
{
    public class LoginActivityViewModel : BaseViewModel
    {
        
        private readonly IDataCollectionAuthentication dataCollectionAuthentication;
        private readonly IPasswordHasher passwordHasher;
        readonly IViewModelNavigationService viewModelNavigationService;

        public LoginActivityViewModel(
            IDataCollectionAuthentication dataCollectionAuthentication, 
            IPasswordHasher passwordHasher,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.dataCollectionAuthentication = dataCollectionAuthentication;
            this.passwordHasher = passwordHasher;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public string Login { get; private set; }
        public string Password { get; private set; }

        public bool HasKnownUsers { get; private set; }
        public string KnownUsers { get; private set; }
        
        private bool areCredentialsWrong = false;
        public bool AreCredentialsWrong
        {
            get { return this.areCredentialsWrong; }
            set { this.areCredentialsWrong = value; this.RaisePropertyChanged(); }
        }

        private bool shouldActivationButtonBeVisible = false;
        public bool ShouldActivationButtonBeVisible
        {
            get { return this.shouldActivationButtonBeVisible; }
            set { this.shouldActivationButtonBeVisible = value; this.RaisePropertyChanged(); }
        }

        private int countOfUnsuccessfulLogins = 0;
        
        public async void Init()
        {
            List<string> previousLoggedInUserNames = await this.dataCollectionAuthentication.GetKnownUsers() ?? new List<string>();

            this.HasKnownUsers = previousLoggedInUserNames.Any();
            this.KnownUsers = string.Join(", ", previousLoggedInUserNames);

            if (previousLoggedInUserNames.Count == 1 && this.Login.IsNullOrEmpty())
            {
                this.Login = previousLoggedInUserNames.First();
            }
            this.countOfUnsuccessfulLogins = 0;
            this.ShouldActivationButtonBeVisible = false;

#if DEBUG
            this.Login = "interviewer1";
            this.Password = "Qwerty1234";
#endif
        }

        public IMvxCommand LoginCommand
        {
            get { return new MvxCommand(this.StartLogin); }
        }

        public IMvxCommand ActivationCommand
        {
            get { return new MvxCommand(this.StartActivation); }
        }

        public IMvxCommand NavigateToSynchronizationCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateTo<SynchronizationViewModel>()); }
        }

        public IMvxCommand NavigateToSettingsCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateTo<SettingsViewModel>()); }
        }

        private async void StartLogin()
        {
            var result = await Mvx.Resolve<IDataCollectionAuthentication>().LogOnAsync(this.Login, this.Password);
            if (result)
            {
                this.viewModelNavigationService.NavigateToDashboard();
                return;
            }

            this.AreCredentialsWrong = true;
            this.countOfUnsuccessfulLogins++;
            if (this.countOfUnsuccessfulLogins > 3)
            {
                this.ShouldActivationButtonBeVisible = true;
            }
        }

        private void StartActivation()
        {
            this.viewModelNavigationService.NavigateTo<SynchronizationViewModel>(new
            {
                login = this.Login,
                passwordHash = this.passwordHasher.Hash(this.Password)
            });
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}