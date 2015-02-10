using Android.App;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Mvvm.Views
{
    [Activity(NoHistory = true, MainLauncher = true, Theme = "@style/Theme.Splash")]
    public class SplashActivityView : BaseActivityView<SplashViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.Splash; }
        }
    }
}