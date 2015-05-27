using Android.App;
using Android.Content.PM;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : BaseActivity<SplashViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.splash; }
        }
    }
}