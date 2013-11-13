using Android.App;
using Android.Content.PM;
using Android.OS;
using Cirrious.MvvmCross.Droid.Simple;

namespace WB.UI.QuestionnaireTester
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class HomeActivity : MvxSimpleBindingActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
        }
    }
}