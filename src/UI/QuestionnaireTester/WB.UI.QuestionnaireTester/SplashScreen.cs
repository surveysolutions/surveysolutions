using Android.App;
using Android.Content.PM;
using Cirrious.MvvmCross.Droid.Views;

namespace WB.UI.QuestionnaireTester
{
    [Activity(Label = "Tester", MainLauncher = true, NoHistory = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class SplashScreen : MvxSplashScreenActivity
    {
        public SplashScreen()
            : base(Resource.Layout.SplashScreen)
        {
        }
   }
}