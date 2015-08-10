using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class SearchQuestionnairesViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        public SearchQuestionnairesViewModel(IViewModelNavigationService viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public override void NavigateToPreviousViewModel()
        {
            this.viewModelNavigationService.NavigateTo<DashboardViewModel>();
        }
    }
}