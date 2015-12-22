using System;
//using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class TabletDiagnosticService: ITabletDiagnosticService
    {
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public TabletDiagnosticService(
            IInterviewerSettings interviewerSettings, 
            IFileSystemAccessor fileSystemAccessor)
        {
            this.interviewerSettings = interviewerSettings;
            this.fileSystemAccessor = fileSystemAccessor;
        }

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

        public async Task UpdateTheApp(string url)
        {
            var applicationFileName = "interviewer.apk";

            string pathTofile =
                fileSystemAccessor.CombinePath(this.fileSystemAccessor.CombinePath(this.interviewerSettings.ExternalStorageDirectory, "download"),
                    applicationFileName);
            // generate unique name instead of delete
            if (this.fileSystemAccessor.IsFileExists(pathTofile))
                this.fileSystemAccessor.DeleteFile(pathTofile);

            var client = new WebClient();
            var uri = new Uri(new Uri(url), "/api/InterviewerSync/GetLatestVersion");
            await Task.Run(() => client.DownloadFile(uri, pathTofile));

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