using Android.OS;
using Android.Support.Graphics.Drawable;
using Android.Views;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.ViewModels;
using NLog;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.Activities
{
    [MvxActivityPresentation]
    public abstract class BaseActivity<TViewModel> : MvxAppCompatActivity<TViewModel> where TViewModel : class, IMvxViewModel
    {
        
        protected abstract int ViewResourceId { get; }
        private Logger log;

        protected override void OnCreate(Bundle bundle)
        {
            log = LogManager.GetLogger(this.GetType().Name);
            log.Trace("Create");
            base.OnCreate(bundle);
            CrossCurrentActivity.Current.Init(this, bundle);
        }

        protected override void OnStart()
        {
            log.Trace("Start");
            base.OnStart();
        }

        protected override void OnResume()
        {
            log.Trace("Resume");
            base.OnResume();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            log.Trace($"OnRequestPermissionsResult permissions {string.Join(',', permissions)} grantResults {string.Join(',', grantResults)}");

            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnPause()
        {
            log.Trace("Pause");
            base.OnPause();
        }

        protected override void OnStop()
        {
            log.Trace("Stop");
            base.OnStop();
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.SetContentView(this.ViewResourceId);
        }

        public override void OnLowMemory()
        {
            this.TryWriteMemoryInformationToLog("LowMemory notification");
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
                log.Error($"{message} RAM: {AndroidInformationUtils.GetRAMInformation()} Disk: {AndroidInformationUtils.GetDiskInformation()}");
            }
            catch
            {
                // ignore if we can get info about RAM and Disk
            }
        }
    }
}
