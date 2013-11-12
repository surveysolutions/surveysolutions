using Android.App;
using Android.Content.PM;
using Cirrious.MvvmCross.Droid.Views;

namespace WB.UI.Capi.Tester
{
    [Activity(Label = "CAPI",  MainLauncher = true, NoHistory = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class SplashScreen : MvxSplashScreenActivity
    {
        public SplashScreen()
            : base(Resource.Layout.SplashScreen)
        {
        }
   }
}