using System.Threading.Tasks;
using Android.App;
using Java.Lang;
using WB.UI.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(NoHistory = true, MainLauncher = true, Theme = "@style/Theme.Splash")]
    public class SplashActivityView : BaseActivityView<SplashViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.Splash; }
        }

        protected override async void OnViewModelSet()
        {
            await Task.Run(() =>
            {
                Thread.Sleep(2000);
                ViewModel.Load(); 
            });
        }
    }
}