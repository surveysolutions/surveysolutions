using Android.App;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Mvvm.Views
{
    [Activity(Theme = "@style/Theme.Tester")]
    public class HelpView : BaseActivityView<HelpViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.Help; }
        }
    }
}
