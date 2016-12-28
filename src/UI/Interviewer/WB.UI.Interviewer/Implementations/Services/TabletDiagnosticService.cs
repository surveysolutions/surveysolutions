using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;
using Flurl;
using Plugin.Permissions.Abstractions;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Shared.Enumerator;
using WB.UI.Interviewer.Activities;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class TabletDiagnosticService: ITabletDiagnosticService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPermissions permissions;

        public TabletDiagnosticService(IFileSystemAccessor fileSystemAccessor,
            IPermissions permissions)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.permissions = permissions;
        }

        private Activity CurrentActivity => Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

        public void LaunchShareAction(string title, string info)
        {
            var shareIntent = new Intent(Intent.ActionSend);
            shareIntent.PutExtra(Intent.ExtraText, info);
            shareIntent.SetType("text/plain");
            this.CurrentActivity.StartActivity(Intent.CreateChooser(shareIntent, title));
        }

        public async Task UpdateTheApp(string url, CancellationToken cancellationToken, TimeSpan timeout)
        {
            await this.permissions.AssureHasPermission(Permission.Storage);
            var applicationFileName = "interviewer.apk";
            var pathToRootDirectory = AndroidPathUtils.GetPathToInternalDirectory();
            var downloadFolder = this.fileSystemAccessor.CombinePath(pathToRootDirectory, "download");
            string pathTofile = this.fileSystemAccessor.CombinePath(downloadFolder, applicationFileName);

            if (this.fileSystemAccessor.IsFileExists(pathTofile))
            {
                this.fileSystemAccessor.DeleteFile(pathTofile);
            }

            if (!this.fileSystemAccessor.IsDirectoryExists(downloadFolder))
            {
                this.fileSystemAccessor.CreateDirectory(downloadFolder);
            }

            HttpClient client = new HttpClient();
            client.Timeout = timeout;
            var uri = new Uri(Url.Combine(url, "/api/InterviewerSync/GetLatestVersion"));

            var response = await client.GetAsync(uri, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            response.EnsureSuccessStatusCode();

            byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
            cancellationToken.ThrowIfCancellationRequested();
            this.fileSystemAccessor.WriteAllBytes(pathTofile, responseBytes);
            cancellationToken.ThrowIfCancellationRequested();

            var topActivity = this.CurrentActivity;
            var uriForFile = FileProvider.GetUriForFile(topActivity.BaseContext, topActivity.ApplicationContext.PackageName + ".fileprovider", new Java.IO.File(pathTofile));

            var intent = ShareCompat.IntentBuilder.From(topActivity)
                .SetStream(uriForFile)
                .Intent
                .SetAction(Intent.ActionView)
                .SetDataAndType(uriForFile, "application/vnd.android.package-archive")
                .AddFlags(ActivityFlags.GrantReadUriPermission);

            Application.Context.StartActivity(intent);
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