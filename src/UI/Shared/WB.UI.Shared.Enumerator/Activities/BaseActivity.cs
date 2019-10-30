using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Android.Content.PM;
using Android.OS;
using Android.Support.Graphics.Drawable;
using Android.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmCross;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.ViewModels;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.Activities
{
    static class CrashReportingConfiguration
    {
        public static bool IsCrashReportingConfigured;
        public static object CrashLockObject = new object();
    }

    [MvxActivityPresentation]
    public abstract class BaseActivity<TViewModel> : MvxAppCompatActivity<TViewModel> where TViewModel : class, IMvxViewModel
    {
        
        protected abstract int ViewResourceId { get; }
        private ILogger log;

        protected override void OnCreate(Bundle bundle)
        {
            if (!CrashReportingConfiguration.IsCrashReportingConfigured)
            {
                lock (CrashReportingConfiguration.CrashLockObject)
                {
                    if (!CrashReportingConfiguration.IsCrashReportingConfigured)
                    {
                        Crashes.GetErrorAttachments = report =>
                        {
                            var result =  new List<ErrorAttachmentLog>();
                            var pathToLocalDirectory = AndroidPathUtils.GetPathToInternalDirectory();
                            
                            var lastLogFile = Path.Combine(pathToLocalDirectory, "Logs", report.AppErrorTime.ToString("yyyy-MM-dd.log"));
                            if (File.Exists(lastLogFile))
                            {
                                result.Add(ErrorAttachmentLog.AttachmentWithText(File.ReadAllText(lastLogFile),"Log.txt"));
                            }

                            return result;
                        };

                        ApplicationInfo ai = PackageManager.GetApplicationInfo(PackageName, PackageInfoFlags.MetaData);
                        var myApiKey = ai.MetaData.GetString("net.hockeyapp.android.appIdentifier");
                        AppCenter.Start(myApiKey, typeof(Analytics), typeof(Crashes));

                        CrashReportingConfiguration.IsCrashReportingConfigured = true;
                    }
                }
            }
            

            log = Mvx.IoCProvider.Resolve<ILoggerProvider>().GetForType(this.GetType());
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
