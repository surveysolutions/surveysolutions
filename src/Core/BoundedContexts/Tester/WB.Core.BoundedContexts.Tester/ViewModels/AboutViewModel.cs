using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        public AboutViewModel(IViewModelNavigationService viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public override void NavigateToPreviousViewModel()
        {
            this.viewModelNavigationService.NavigateTo<DashboardViewModel>();
        }
    }
}