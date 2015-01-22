using Android.App;
using WB.UI.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(Theme = "@style/Theme.Tester", Icon = "@drawable/back_button_tester")]
    public class SettingsView : BaseActivityView<SettingsViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.Settings; }
        }
    }
}
