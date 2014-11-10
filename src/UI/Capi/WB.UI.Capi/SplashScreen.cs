using Android.App;
using Android.Content.PM;
using Cirrious.MvvmCross.Droid.Views;

namespace WB.UI.Capi
{
    [Activity(MainLauncher = true, NoHistory = true)]
    public class SplashScreen : MvxSplashScreenActivity
    {
        public SplashScreen()
            : base(Resource.Layout.SplashScreen) {}
    }
}