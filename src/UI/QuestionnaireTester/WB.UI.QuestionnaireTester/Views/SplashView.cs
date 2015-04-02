using Android.App;
using Android.Content.PM;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivityView : BaseActivityView<SplashViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.splash; }
        }
    }
}