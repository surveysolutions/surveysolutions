using Android.App;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(Theme = "@style/Theme.Tester")]
    public class SettingsView : BaseActivityView<SettingsViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.Settings; }
        }
    }
}
