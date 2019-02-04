using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class EnumeratorSplashScreenAppCompatActivity<TSetup, TApplication> : MvxSplashScreenAppCompatActivity<TSetup, TApplication>
        where TSetup : MvxAppCompatSetup<TApplication>, new()
        where TApplication : class, IMvxApplication, new()
    {
        protected EnumeratorSplashScreenAppCompatActivity(int resourceId) : base(resourceId)
        {
        }
    }
}
