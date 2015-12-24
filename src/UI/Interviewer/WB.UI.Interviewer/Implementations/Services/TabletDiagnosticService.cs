using System;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using Flurl;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Shared.Enumerator;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class TabletDiagnosticService: ITabletDiagnosticService
    {
        private readonly IAsynchronousFileSystemAccessor asynchronousFileSystemAccessor;

        public TabletDiagnosticService(IAsynchronousFileSystemAccessor asynchronousFileSystemAccessor)
        {
            this.asynchronousFileSystemAccessor = asynchronousFileSystemAccessor;
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
            var pathToExternalDirectory = AndroidPathUtils.GetPathToExternalDirectory();
            var downloadFolder = this.asynchronousFileSystemAccessor.CombinePath(pathToExternalDirectory, "download");
            string pathTofile = this.asynchronousFileSystemAccessor.CombinePath(downloadFolder, applicationFileName);

            if (await this.asynchronousFileSystemAccessor.IsFileExistsAsync(pathTofile))
            {
                await this.asynchronousFileSystemAccessor.DeleteFileAsync(pathTofile);
            }

            HttpClient client = new HttpClient();
            
            var uri = new Uri(Url.Combine(url, "/api/InterviewerSync/GetLatestVersion"));

            var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
            await this.asynchronousFileSystemAccessor.WriteAllBytesAsync(pathTofile, responseBytes);

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