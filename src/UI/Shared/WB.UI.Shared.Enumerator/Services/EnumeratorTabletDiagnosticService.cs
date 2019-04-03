using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using MvvmCross;
using MvvmCross.Platforms.Android;
using Plugin.Permissions.Abstractions;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.UI.Shared.Enumerator.Services
{
    public abstract class EnumeratorTabletDiagnosticService: ITabletDiagnosticService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPermissions permissions;
        private readonly ISynchronizationService synchronizationService;
        private readonly IDeviceSettings deviceSettings;
        private readonly IArchivePatcherService archivePatcherService;
        private readonly ILogger logger;
        private readonly IViewModelNavigationService navigationService;

        protected EnumeratorTabletDiagnosticService(IFileSystemAccessor fileSystemAccessor,
            IPermissions permissions,
            ISynchronizationService synchronizationService,
            IDeviceSettings deviceSettings,
            IArchivePatcherService archivePatcherService,
            ILogger logger,
            IViewModelNavigationService navigationService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.permissions = permissions;
            this.synchronizationService = synchronizationService;
            this.deviceSettings = deviceSettings;
            this.archivePatcherService = archivePatcherService;
            this.logger = logger;
            this.navigationService = navigationService;
        }

        private Activity CurrentActivity => Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

        public void LaunchShareAction(string title, string info)
        {
            var shareIntent = new Intent(Intent.ActionSend);
            shareIntent.PutExtra(Intent.ExtraText, info);
            shareIntent.SetType("text/plain");
            this.CurrentActivity.StartActivity(Intent.CreateChooser(shareIntent, title));
        }

        public async Task UpdateTheApp(CancellationToken cancellationToken, 
            bool continueIfNoPatch = true,
            IProgress<TransferProgress> onDownloadProgressChanged = null)
        {
            await this.permissions.AssureHasPermission(Permission.Storage);

            var pathToRootDirectory = Build.VERSION.SdkInt < BuildVersionCodes.N
                ? AndroidPathUtils.GetPathToExternalDirectory()
                : AndroidPathUtils.GetPathToInternalDirectory();

            var downloadFolder = this.fileSystemAccessor.CombinePath(pathToRootDirectory, "download");

            string pathToPatch = this.fileSystemAccessor.CombinePath(downloadFolder, "application.patch");
            string pathToNewApk = this.fileSystemAccessor.CombinePath(downloadFolder, "application.apk");
            string pathToOldApk = this.deviceSettings.InstallationFilePath;

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
                patchOrFullApkBytes = await this.synchronizationService.GetApplicationPatchAsync(cancellationToken, onDownloadProgressChanged);
            }
            catch (SynchronizationException ex) when (ex.InnerException is RestException rest)
            {
                if (rest.StatusCode != HttpStatusCode.NotFound)
                    throw;
            }

            cancellationToken.ThrowIfCancellationRequested();

            async Task GetWithFullApk()
            {
                cancellationToken.ThrowIfCancellationRequested();
                patchOrFullApkBytes = await this.synchronizationService.GetApplicationAsync(cancellationToken, onDownloadProgressChanged);
                cancellationToken.ThrowIfCancellationRequested();

                if (this.fileSystemAccessor.IsFileExists(pathToNewApk))
                {
                    this.fileSystemAccessor.DeleteFile(pathToNewApk);
                }

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
                catch(Exception e)
                {
                    this.logger.Error("Were not able to apply delta patch. ", e);

                    if (continueIfNoPatch)
                        await GetWithFullApk();
                }
            }
            else
            {
                if (continueIfNoPatch)
                    await GetWithFullApk();
            }

            if (patchOrFullApkBytes == null)
                return;

            cancellationToken.ThrowIfCancellationRequested();

            this.navigationService.InstallNewApp(pathToNewApk);
        }

        public void RestartTheApp()
        {
            Intent intent = new Intent(this.CurrentActivity, this.SplashActivityType);
            intent.AddFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(intent);
            Process.KillProcess(Process.MyPid());
        }

        protected abstract Type SplashActivityType { get; }
    }
}
