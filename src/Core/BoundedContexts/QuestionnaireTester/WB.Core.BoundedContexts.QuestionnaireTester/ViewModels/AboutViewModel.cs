namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public override void NavigateToPreviousViewModel()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}