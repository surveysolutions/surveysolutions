using Android.App;
using Cirrious.MvvmCross.Droid.Views;

namespace CAPI.Android
{
    using global::Android.Content.PM;

    [Activity(Label = "CAPI", MainLauncher = true, NoHistory = true, Icon = "@drawable/capi",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class SplashScreenActivity : MvxBaseSplashScreenActivity
    {
        public SplashScreenActivity()
            : base(Resource.Layout.SplashScreen)
        {
        }
      /*  protected override void TriggerFirstNavigate()
        {
            CapiApplication.GenerateEvents();
            base.TriggerFirstNavigate();
        }*/

    }
}