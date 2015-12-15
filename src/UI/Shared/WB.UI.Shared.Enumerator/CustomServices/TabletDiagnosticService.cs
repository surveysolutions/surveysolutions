using System;
using System.IO;
using System.Net;
using Android.App;
using Android.Content;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    internal class TabletDiagnosticService: ITabletDiagnosticService
    {
        private Activity CurrentActivity
        {
            get { return Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity; }
        }

        public void LaunchShareAction(string title, string info)
        {
            var shareIntent = new Intent(Intent.ActionSend);
            shareIntent.PutExtra(Intent.ExtraText, info);
            shareIntent.SetType("text/plain");
            this.CurrentActivity.StartActivity(Intent.CreateChooser(shareIntent, title));
        }

        public void UpdateTheApp(string url)
        {
            var applicationFileName = "interviewer.apk";

            string pathTofile =
                Path.Combine(
                    Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "download"),
                    applicationFileName);
            // generate unique name instead of delete
            if (File.Exists(pathTofile))
                File.Delete(pathTofile);

            var client = new WebClient();
            var uri = new Uri(new Uri(url), "/api/InterviewerSync/GetLatestVersion");
            client.DownloadFile(uri, pathTofile);

            Intent promptInstall =
                new Intent(Intent.ActionView).SetDataAndType(
                    global::Android.Net.Uri.FromFile(new Java.IO.File(pathTofile)),
                    "application/vnd.android.package-archive");
            promptInstall.AddFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(promptInstall);
        }

        public void RestartTheApp()
        {
            Intent intent = this.CurrentActivity.PackageManager.GetLaunchIntentForPackage(this.CurrentActivity.PackageName);
            intent.AddFlags(ActivityFlags.ClearTop);
            Application.Context.StartActivity(intent);
        }
    }
}