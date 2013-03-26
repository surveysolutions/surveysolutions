using Android.App;
using Cirrious.MvvmCross.Droid.Views;

namespace CAPI.Android
{
    [Activity(Label = "CAPI", MainLauncher = true, NoHistory = true, Icon = "@drawable/capi")]
    public class SplashScreenActivity : MvxBaseSplashScreenActivity
    {
        public SplashScreenActivity()
            : base(Resource.Layout.SplashScreen)
        {
        }
        protected override void TriggerFirstNavigate()
        {
            CapiApplication.GenerateEvents();
            base.TriggerFirstNavigate();
        }

    }
}