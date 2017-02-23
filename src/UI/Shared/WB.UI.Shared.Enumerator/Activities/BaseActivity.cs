using Android.App;
using Android.Content;
using Android.OS;
using MvvmCross.Droid.Platform;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platform;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using WB.Core.GenericSubdomains.Portable;
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

        private void TryWriteMemoryInformationToLog(string message)
        {
            try
            {
                Mvx.Error(message + System.Environment.NewLine);
                Mvx.Error($"RAM: {this.GetRAMInformation()} {System.Environment.NewLine}");
                Mvx.Error($"Disk: {this.GetDiskInformation()} {System.Environment.NewLine}");
            }
            catch
            {
                // ignore if we can get info about RAM and Disk
            }
        }

        private string GetRAMInformation()
        {
            ActivityManager activityManager = Application.Context.GetSystemService(Context.ActivityService) as ActivityManager;
            if (activityManager == null)
                return "UNKNOWN";

            ActivityManager.MemoryInfo mi = new ActivityManager.MemoryInfo();
            activityManager.GetMemoryInfo(mi);
            return $"{FileSizeUtils.SizeSuffix(mi.TotalMem)} total, avaliable {(int)(((double)(100 * mi.AvailMem)) / mi.TotalMem)}% ({FileSizeUtils.SizeSuffix(mi.AvailMem)})";
        }

        private string GetDiskInformation()
        {
            string path = global::Android.OS.Environment.DataDirectory.Path;
            StatFs stat = new StatFs(path);
            long blockSize = stat.BlockSizeLong;
            long availableBlocks = stat.AvailableBlocksLong;
            long totalBlocks = stat.BlockCountLong;
            var availableInternalMemorySize = (availableBlocks * blockSize);
            var totalInternalMemorySize = totalBlocks * blockSize;
            return $"{FileSizeUtils.SizeSuffix(totalInternalMemorySize)} total, avaliable {(int)(((double)(100 * availableInternalMemorySize)) / totalInternalMemorySize)}% ({FileSizeUtils.SizeSuffix(availableInternalMemorySize)})";
        }

    }
}