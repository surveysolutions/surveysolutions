using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SearchQuestionnairesViewModel : BaseViewModel
    {
        public override void NavigateToPreviousViewModel()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}