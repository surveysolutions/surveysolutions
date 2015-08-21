using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Capi.ViewModel
{
    public class DashboardViewModel : BaseViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public void Init()
        {
            if (!Mvx.Resolve<IDataCollectionAuthentication>().IsLoggedIn)
            {
                this.viewModelNavigationService.NavigateTo<LoginActivityViewModel>();
                return;
            }

            this.LoggedInUserName = Mvx.Resolve<IDataCollectionAuthentication>().CurrentUser.Name;
        }

        private string loggedInUserName;
        public string LoggedInUserName
        {
            get { return this.loggedInUserName; }
            set { this.loggedInUserName = value; RaisePropertyChanged(); }
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