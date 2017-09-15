using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;
using Plugin.Permissions.Abstractions;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Interviewer.Activities;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class TabletDiagnosticService: ITabletDiagnosticService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPermissions permissions;
        private readonly ISynchronizationService synchronizationService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IArchivePatcherService archivePatcherService;

        public TabletDiagnosticService(IFileSystemAccessor fileSystemAccessor,
            IPermissions permissions,
            ISynchronizationService synchronizationService,
            IInterviewerSettings interviewerSettings,
            IArchivePatcherService archivePatcherService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.permissions = permissions;
            this.synchronizationService = synchronizationService;
            this.interviewerSettings = interviewerSettings;
            this.archivePatcherService = archivePatcherService;
        }

        private Activity CurrentActivity => Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

        public void LaunchShareAction(string title, string info)
        {
            var shareIntent = new Intent(Intent.ActionSend);
            shareIntent.PutExtra(Intent.ExtraText, info);
            shareIntent.SetType("text/plain");
            this.CurrentActivity.StartActivity(Intent.CreateChooser(shareIntent, title));
        }

        public async Task UpdateTheApp(CancellationToken cancellationToken)
        {
            await this.permissions.AssureHasPermission(Permission.Storage);

            var pathToRootDirectory = Build.VERSION.SdkInt < BuildVersionCodes.N
                ? AndroidPathUtils.GetPathToExternalDirectory()
                : AndroidPathUtils.GetPathToInternalDirectory();

            var downloadFolder = this.fileSystemAccessor.CombinePath(pathToRootDirectory, "download");

            string pathToPatch = this.fileSystemAccessor.CombinePath(downloadFolder, "interviewer.patch");
            string pathToNewApk = this.fileSystemAccessor.CombinePath(downloadFolder, "interviewer.apk");
            string pathToOldApk = this.interviewerSettings.InstallationFilePath;
            

            if (this.fileSystemAccessor.IsFileExists(pathToPatch))
            {
                this.fileSystemAccessor.DeleteFile(pathToPatch);
            }

            if (this.fileSystemAccessor.IsFileExists(pathToNewApk))
            {
                this.fileSystemAccessor.DeleteFile(pathToNewApk);
            }

            if (!this.fileSystemAccessor.IsDirectoryExists(downloadFolder))
            {
                this.fileSystemAccessor.CreateDirectory(downloadFolder);
            }
            
            byte[] patchOrFullApkBytes = null;

            try
            {
                patchOrFullApkBytes = await this.synchronizationService.GetApplicationPatchAsync(cancellationToken);
            }
            catch (SynchronizationException ex) when (ex.InnerException is RestException rest)
            {
                if (rest.StatusCode != HttpStatusCode.NotFound) throw;
            }

            cancellationToken.ThrowIfCancellationRequested();

            async Task UpdateWithFullApk()
            {
                cancellationToken.ThrowIfCancellationRequested();
                patchOrFullApkBytes = await this.synchronizationService.GetApplicationAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                this.fileSystemAccessor.WriteAllBytes(pathToNewApk, patchOrFullApkBytes);
            }

            if (patchOrFullApkBytes != null)
            {
                try
                {
                    this.fileSystemAccessor.WriteAllBytes(pathToPatch, patchOrFullApkBytes);
                    cancellationToken.ThrowIfCancellationRequested();

                    this.archivePatcherService.ApplyPath(pathToOldApk, pathToPatch, pathToNewApk);
                }
                catch
                {
                    await UpdateWithFullApk();
                }
            }
            else
            {
                await UpdateWithFullApk();
            }

            cancellationToken.ThrowIfCancellationRequested();

            Intent promptInstall;
            if (Build.VERSION.SdkInt < BuildVersionCodes.N)
            {
                promptInstall =
                    new Intent(Intent.ActionView)
                        .SetDataAndType(global::Android.Net.Uri.FromFile(new Java.IO.File(pathToNewApk)), "application/vnd.android.package-archive")
                        .AddFlags(ActivityFlags.NewTask)
                        .AddFlags(ActivityFlags.GrantReadUriPermission);
            }
            else
            {
                var topActivity = this.CurrentActivity;
                var uriForFile = FileProvider.GetUriForFile(topActivity.BaseContext, topActivity.ApplicationContext.PackageName + ".fileprovider", new Java.IO.File(pathToNewApk));

                promptInstall = ShareCompat.IntentBuilder.From(topActivity)
                    .SetStream(uriForFile)
                    .Intent
                    .SetAction(Intent.ActionView)
                    .SetDataAndType(uriForFile, "application/vnd.android.package-archive")
                    .AddFlags(ActivityFlags.GrantReadUriPermission);
            }

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