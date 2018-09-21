using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Plugin.Permissions.Abstractions;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class DownloadInterviewerApplications : SynchronizationStep
    {
        private readonly ISupervisorSynchronizationService synchronizationService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPermissionsService permissions;
        private readonly ISupervisorSettings supervisorSettings;

        public DownloadInterviewerApplications(int sortOrder,
            ILogger logger,
            ISupervisorSynchronizationService synchronizationService,
            IFileSystemAccessor fileSystemAccessor,
            IPermissionsService permissions,
            ISupervisorSettings supervisorSettings) :
            base(sortOrder, synchronizationService, logger)
        {
            this.synchronizationService = synchronizationService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.permissions = permissions;
            this.supervisorSettings = supervisorSettings;
        }

        public override async Task ExecuteAsync()
        {
            if (!this.supervisorSettings.DownloadUpdatesForInterviewerApp) return;

            var latestVersionOfSupervisorApp = await this.synchronizationService.GetLatestApplicationVersionAsync(Context.CancellationToken);
            if (latestVersionOfSupervisorApp != this.supervisorSettings.GetApplicationVersionCode()) return;

            Context.Progress.Report(new SyncProgressInfo
            {
                Title = SupervisorUIResources.Synchronization_Download_Interviewer_Apps,
                Status = SynchronizationStatus.Download
            });

            await this.permissions.AssureHasPermission(Permission.Storage);

            var apksBySupervisorAppVersion = this.PreparePathToInterviewerApksDirectoryBySupervisorAppVersion();

            await DownloadInterviewerApksAsync(apksBySupervisorAppVersion);

        }

        private string PreparePathToInterviewerApksDirectoryBySupervisorAppVersion()
        {
            var interviewerApksDirectory = this.supervisorSettings.InterviewerApplicationsDirectory;

            if (!this.fileSystemAccessor.IsDirectoryExists(interviewerApksDirectory))
                this.fileSystemAccessor.CreateDirectory(interviewerApksDirectory);

            var sAppVersion = this.supervisorSettings.GetApplicationVersionCode().ToString();
            var apksBySupervisorAppVersion = this.fileSystemAccessor.CombinePath(interviewerApksDirectory, sAppVersion);

            if (!this.fileSystemAccessor.IsDirectoryExists(apksBySupervisorAppVersion))
                this.fileSystemAccessor.CreateDirectory(apksBySupervisorAppVersion);

            // remove old patches by old supervisor app version
            this.fileSystemAccessor.GetDirectoriesInDirectory(interviewerApksDirectory)
                .Where(x => x != apksBySupervisorAppVersion)
                .ForEach(x => this.fileSystemAccessor.DeleteDirectory(x));

            return apksBySupervisorAppVersion;
        }

        private async Task DownloadInterviewerApksAsync(string interviewerApksDirectory)
        {
            Stopwatch sw = null;

            try
            {
                var interviewerAppFilePath = this.fileSystemAccessor.CombinePath(interviewerApksDirectory, "interviewer.apk");
                var interviewerWithMapsAppFilePath = this.fileSystemAccessor.CombinePath(interviewerApksDirectory, "interviewer.maps.apk");

                if (!this.fileSystemAccessor.IsFileExists(interviewerAppFilePath))
                {
                    var interviewerApk = await this.synchronizationService.GetInterviewerApplicationAsync(Context.CancellationToken,
                        new Progress<TransferProgress>(downloadProgress => { UpdateProgress(downloadProgress, ref sw); }));

                    if (interviewerApk != null)
                        this.fileSystemAccessor.WriteAllBytes(interviewerAppFilePath, interviewerApk);
                }

                sw = null;

                if (!this.fileSystemAccessor.IsFileExists(interviewerWithMapsAppFilePath))
                {
                    var interviewerWithMapsApk = await this.synchronizationService.GetInterviewerApplicationWithMapsAsync(Context.CancellationToken,
                            new Progress<TransferProgress>(downloadProgress => { UpdateProgress(downloadProgress, ref sw); }));

                    if (interviewerWithMapsApk != null)
                        this.fileSystemAccessor.WriteAllBytes(interviewerWithMapsAppFilePath, interviewerWithMapsApk);
                }
            }
            catch (Exception exc)
            {
                this.logger.Error("Error on downloading interviewer apks", exc);
            }
        }

        private void UpdateProgress(TransferProgress downloadProgress, ref Stopwatch stopWatch)
        {
            if (stopWatch == null) stopWatch = Stopwatch.StartNew();
            if (downloadProgress.ProgressPercentage % 1 != 0) return;

            var receivedKilobytes = downloadProgress.BytesReceived.Bytes();
            var totalKilobytes = (downloadProgress.TotalBytesToReceive ?? 0).Bytes();

            Context.Progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_DownloadApplication,
                Description = string.Format(
                    InterviewerUIResources.Synchronization_DownloadApplication_Description,
                    receivedKilobytes.Humanize("00.00 MB"),
                    totalKilobytes.Humanize("00.00 MB"),
                    receivedKilobytes.Per(stopWatch.Elapsed).Humanize("00.00"),
                    (int) downloadProgress.ProgressPercentage),
                Status = SynchronizationStatus.Download,
                Stage = SyncStage.DownloadApplication,
                StageExtraInfo = new Dictionary<string, string>()
                {
                    {"receivedKilobytes", receivedKilobytes.Humanize("00.00 MB")},
                    {"totalKilobytes", totalKilobytes.Humanize("00.00 MB")},
                    {"receivingRate", receivedKilobytes.Per(stopWatch.Elapsed).Humanize("00.00")},
                    {"progressPercentage", ((int) downloadProgress.ProgressPercentage).ToString()}
                }
            });
        }
    }
}
