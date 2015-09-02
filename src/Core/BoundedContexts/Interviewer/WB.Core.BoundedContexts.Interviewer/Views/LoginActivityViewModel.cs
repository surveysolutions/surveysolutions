using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class LoginActivityViewModel : BaseViewModel
    {
        private readonly IDataCollectionAuthentication dataCollectionAuthentication;
        private readonly IPasswordHasher passwordHasher;
        readonly IDataCollectionAuthentication authenticationService;
        readonly IViewModelNavigationService viewModelNavigationService;

        public LoginActivityViewModel(
            IDataCollectionAuthentication dataCollectionAuthentication, 
            IPasswordHasher passwordHasher,
            IDataCollectionAuthentication authenticationService,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.dataCollectionAuthentication = dataCollectionAuthentication;
            this.passwordHasher = passwordHasher;
            this.authenticationService = authenticationService;
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

        public bool ShouldActivationButtonBeVisible = true;
        
        public async void Init()
        {
            List<string> previousLoggedInUserNames = await this.dataCollectionAuthentication.GetKnownUsers() ?? new List<string>();

            this.HasKnownUsers = previousLoggedInUserNames.Any();
            this.KnownUsers = string.Join(", ", previousLoggedInUserNames);

            if (previousLoggedInUserNames.Count == 1 && this.Login.IsNullOrEmpty())
            {
                this.Login = previousLoggedInUserNames.First();
            }
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

        private async void StartLogin()
        {
            var result = await this.authenticationService.LogOnAsync(this.Login, this.Password);
            if (result)
            {
                this.viewModelNavigationService.NavigateToDashboard();
                return;
            }

            this.AreCredentialsWrong = true;
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}