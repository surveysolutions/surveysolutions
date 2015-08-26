using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Interviewer.ViewModel
{
    public class DashboardViewModel : BaseViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;
        readonly IDataCollectionAuthentication authenticationService;

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IDataCollectionAuthentication authenticationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.authenticationService = authenticationService;
        }

        public void Init()
        {
            if (!authenticationService.IsLoggedIn)
            {
                this.viewModelNavigationService.NavigateTo<LoginActivityViewModel>();
                return;
            }

            this.LoggedInUserName = authenticationService.CurrentUser.Name;
        }

        private string loggedInUserName;
        public string LoggedInUserName
        {
            get { return this.loggedInUserName; }
            set { this.loggedInUserName = value; this.RaisePropertyChanged(); }
        }

        public IMvxCommand NavigateToSettingsCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateTo<SettingsViewModel>()); }
        }

        public IMvxCommand NavigateToSynchronizationCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateTo<SynchronizationViewModel>()); }
        }

        public IMvxCommand SignOutCommand
        {
            get { return new MvxCommand(this.SignOut); }
        }

        void SignOut()
        {
            Mvx.Resolve<IDataCollectionAuthentication>().LogOff();
            this.viewModelNavigationService.NavigateTo<LoginActivityViewModel>();
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}