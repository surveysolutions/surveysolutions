namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    internal class PrefilledQuestionsViewModel : BaseViewModel
    {
        public override void NavigateToPreviousViewModel()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}