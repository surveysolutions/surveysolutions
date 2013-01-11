using Android.App;
using Cirrious.MvvmCross.Droid.Views;

namespace AndroidApp
{
    [Activity(Label = "CAPI", MainLauncher = true, NoHistory = true, Icon = "@drawable/capi")]
    public class SplashScreenActivity : MvxBaseSplashScreenActivity
    {
        public SplashScreenActivity()
            : base(Resource.Layout.SplashScreen)
        {
        }
    }
}