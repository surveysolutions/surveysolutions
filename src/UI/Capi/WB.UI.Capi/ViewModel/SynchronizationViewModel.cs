using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Capi.ViewModel
{
    public class SynchronizationViewModel : BaseViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;
        public SynchronizationViewModel(IViewModelNavigationService viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public string Login { get; private set; }
        public string Password { get; private set; }

        public void Init(string login, string passwordHash)
        {
            this.Login = login;
            this.Password = passwordHash;
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }

        public IMvxCommand NavigateToSettingsCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateTo<SettingsViewModel>()); }
        }

        public IMvxCommand NavigateToDashboardCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateToDashboard()); }
        }
    }
}