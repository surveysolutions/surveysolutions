using WB.Core.BoundedContexts.QuestionnaireTester.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
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