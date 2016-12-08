using System;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;
using Flurl;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Shared.Enumerator;
using WB.UI.Interviewer.Activities;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class TabletDiagnosticService: ITabletDiagnosticService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        public TabletDiagnosticService(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        private Activity CurrentActivity => Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

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
            var pathToExternalDirectory = AndroidPathUtils.GetPathToInternalDirectory();
            var downloadFolder = this.fileSystemAccessor.CombinePath(pathToExternalDirectory, "download");
            string pathTofile = this.fileSystemAccessor.CombinePath(downloadFolder, applicationFileName);

            if (this.fileSystemAccessor.IsFileExists(pathTofile))
            {
                this.fileSystemAccessor.DeleteFile(pathTofile);
            }

            HttpClient client = new HttpClient();
            
            var uri = new Uri(Url.Combine(url, "/api/InterviewerSync/GetLatestVersion"));

            var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
            this.fileSystemAccessor.WriteAllBytes(pathTofile, responseBytes);

            Intent promptInstall =
                new Intent(Intent.ActionView).SetDataAndType(
                    global::Android.Net.Uri.FromFile(new Java.IO.File(pathTofile)),
                    "application/vnd.android.package-archive");
            promptInstall.AddFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(promptInstall);
        }

        public void RestartTheApp()
        {
            Intent intent = new Intent(this.CurrentActivity,
                typeof(SplashActivity));
            intent.AddFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(intent);
            Process.KillProcess(Process.MyPid());
        }
    }
}