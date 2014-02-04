using Android.App;
using Android.Content.PM;
using Android.OS;
using Cirrious.MvvmCross.Droid.Views;

namespace WB.UI.QuestionnaireTester
{
    [Activity(MainLauncher = true, NoHistory = true, Theme = "@style/Theme.SplashBackground")]
    public class SplashScreenActivity : MvxSplashScreenActivity
    {
        public SplashScreenActivity()
            : base(Resource.Layout.SplashScreen)
        {
        }
    }
}