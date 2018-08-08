using Android.OS;
using Android.Support.Graphics.Drawable;
using Android.Views;
using HockeyApp.Android;
using MvvmCross;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Logging;
using MvvmCross.Platforms.Android.Core;
using MvvmCross.ViewModels;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class BaseActivity<TViewModel> : MvxAppCompatActivity<TViewModel> where TViewModel : class, IMvxViewModel
    {
        protected abstract int ViewResourceId { get; }

        protected override void OnCreate(Bundle bundle)
        {
            var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(ApplicationContext);
            setup.EnsureInitialized();
            CrossCurrentActivity.Current.Init(this, bundle);
            base.OnCreate(bundle);
        }

        protected override void OnResume()
        {
            CrashManager.Register(this, new AutoSendingCrashListener());
            base.OnResume();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.SetContentView(this.ViewResourceId);
        }

        public override void OnLowMemory()
        {
            this.TryWriteMemoryInformationToLog("LowMemory natification");
            base.OnLowMemory();
        }

        protected override void OnDestroy()
        {
            TryWriteMemoryInformationToLog($"Destroyed Activity {this.GetType().Name}");
            base.OnDestroy();
        }

        protected void SetMenuItemIcon(IMenu menu, int itemId, int drawableId)
        {
            var item = menu.FindItem(itemId);
            var drawable = VectorDrawableCompat.Create(Resources, drawableId, Theme);
            item.SetIcon(drawable);
        }

        private void TryWriteMemoryInformationToLog(string message)
        {
            try
            {
                var mvxLogProvider = Mvx.Resolve<IMvxLogProvider>();
                var log = mvxLogProvider.GetLogFor(this.GetType().Name);
                log.Error(message + System.Environment.NewLine);
                log.Error($"RAM: {AndroidInformationUtils.GetRAMInformation()} {System.Environment.NewLine}");
                log.Error($"Disk: {AndroidInformationUtils.GetDiskInformation()} {System.Environment.NewLine}");
            }
            catch
            {
                // ignore if we can get info about RAM and Disk
            }
        }
    }
}
