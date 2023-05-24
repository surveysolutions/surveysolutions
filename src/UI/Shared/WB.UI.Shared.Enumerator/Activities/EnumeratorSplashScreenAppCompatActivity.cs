using MvvmCross.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class EnumeratorSplashScreenAppCompatActivity<TSetup, TApplication> : MvvmCross.Platforms.Android.Views.MvxSplashScreenActivity<TSetup, TApplication>
        where TSetup : MvvmCross.Platforms.Android.Core.MvxAndroidSetup<TApplication>, new()
        where TApplication : class, IMvxApplication, new()
    {
        protected EnumeratorSplashScreenAppCompatActivity(int resourceId) : base(resourceId)
        {
        }
    }
}
