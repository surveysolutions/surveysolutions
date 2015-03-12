using Android.App;
using Android.Content.PM;
using WB.UI.Interviewer.Droid.MvxDroidAdaptation;

namespace WB.UI.Interviewer.Droid
{
    [Activity(
        Label = "Welcome to Xamarin.Forms"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , NoHistory = true
        , ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreen : MvxFormsSplashScreenActivity
    {
        public SplashScreen()
            : base(Resource.Layout.SplashScreen)
        {
        }
    }
}