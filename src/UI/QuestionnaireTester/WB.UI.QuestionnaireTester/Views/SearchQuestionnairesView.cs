using Android.App;
using Android.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(Label = "", Theme = "@style/GrayAppTheme", HardwareAccelerated = true, WindowSoftInputMode = SoftInput.StateHidden)]
    public class SearchQuestionnairesView : BaseActivityView<SearchQuestionnairesViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.search_questionnaires; }
        }
    }
}