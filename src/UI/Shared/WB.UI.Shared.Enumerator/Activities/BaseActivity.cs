using Android.OS;
using MvvmCross.Droid.Platform;
using MvvmCross.Droid.Support.V7.AppCompat;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class BaseActivity<TViewModel> : MvxAppCompatActivity<TViewModel> where TViewModel : BaseViewModel
    {
        protected abstract int ViewResourceId { get; }

        protected override void OnCreate(Bundle bundle)
        {
            var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(ApplicationContext);
            setup.EnsureInitialized();
            CrossCurrentActivity.Current.Activity = this;
            base.OnCreate(bundle);
        }

        protected override void OnResume()
        {
            base.OnResume();
            CrossCurrentActivity.Current.Activity = this;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.SetContentView(this.ViewResourceId);
        }
    }
}