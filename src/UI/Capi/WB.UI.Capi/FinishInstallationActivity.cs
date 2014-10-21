using Android.App;
using Android.OS;
using Android.Content.PM;
using Cirrious.MvvmCross.Droid.Views;
using WB.UI.Capi.Views;

namespace WB.UI.Capi
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class FinishInstallationActivity : MvxActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            this.DataContext = new FinishIntallationViewModel();
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.FinishInstallation);
        }
    }
}