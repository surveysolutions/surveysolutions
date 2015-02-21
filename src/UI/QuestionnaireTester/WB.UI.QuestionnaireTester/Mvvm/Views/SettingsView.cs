using Android.App;
using Android.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Mvvm.Views
{
    [Activity(Theme = "@style/Theme.Tester", WindowSoftInputMode = SoftInput.AdjustPan)]
    public class SettingsView : BaseActivityView<SettingsViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.Settings; }
        }
    }
}
