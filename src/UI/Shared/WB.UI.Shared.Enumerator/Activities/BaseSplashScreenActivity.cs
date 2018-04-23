using MvvmCross.Platforms.Android.Views;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class BaseSplashScreenActivity : MvxSplashScreenActivity
    {
        protected BaseSplashScreenActivity(int resourceId = 0) : base(resourceId)
        {
        }

        protected override void OnResume()
        {
            CrashReporting.Init(this);
            base.OnResume();
        }
    }
}
