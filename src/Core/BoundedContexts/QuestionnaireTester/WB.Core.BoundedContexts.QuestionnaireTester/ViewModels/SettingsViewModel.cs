namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public override void NavigateToPreviousViewModel()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}