using WB.Core.BoundedContexts.Tester.Services;

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