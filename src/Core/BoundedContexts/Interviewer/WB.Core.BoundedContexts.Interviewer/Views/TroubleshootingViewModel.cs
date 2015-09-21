using Cirrious.MvvmCross.ViewModels;

using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class TroubleshootingViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;

        public TroubleshootingViewModel(IViewModelNavigationService viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public IMvxCommand NavigateToDashboardCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateToDashboard()); }
        }
    }
}
