using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Capi.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;
        public SettingsViewModel(IViewModelNavigationService viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }

        public IMvxCommand NavigateToSynchronizationCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateTo<SynchronizationViewModel>()); }
        }

        public IMvxCommand NavigateToDashboardCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateToDashboard()); }
        }
    }
}