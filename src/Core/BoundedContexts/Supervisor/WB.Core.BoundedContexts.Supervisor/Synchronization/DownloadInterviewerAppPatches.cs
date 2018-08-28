using System;
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
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class DownloadInterviewerAppPatches : SynchronizationStep
    {
        private readonly ISupervisorSynchronizationService synchronizationService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPermissionsService permissions;
        private readonly ISupervisorSettings supervisorSettings;

        public DownloadInterviewerAppPatches(int sortOrder,
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
            Context.Progress.Report(new SyncProgressInfo
            {
                Title = SupervisorUIResources.Synchronization_Download_Interviewer_App_Patches,
                Status = SynchronizationStatus.Download
            });

            await this.permissions.AssureHasPermission(Permission.Storage);

            var patchesBySupervisorAppVersion = GetPathToPatchesDirectoryBySupervisorAppVersion();

            var latestVersionOfInterviewerApp = await this.synchronizationService.GetLatestInterviewerAppVersionAsync(Context.CancellationToken);
            this.supervisorSettings.SetLatestInterviewerAppVersion(latestVersionOfInterviewerApp);
            
            var listOfPatchesInfo = await this.synchronizationService.GetListOfInterviewerAppPatchesAsync(Context.CancellationToken);
            var listOfMissingPatches = listOfPatchesInfo
                .Where(x =>
                {
                    var filePath = this.fileSystemAccessor.CombinePath(patchesBySupervisorAppVersion, x.FileName);

                    return !this.fileSystemAccessor.IsFileExists(filePath) ||
                           // sometimes we have deltas with zero length. just for no hotfix in the future
                           this.fileSystemAccessor.GetFileSize(filePath) == 0;
                })
                .ToArray();

            for(int patchIndex = 0; patchIndex < listOfMissingPatches.Length; patchIndex++)
            {
                await DownloadPatchFileAsync(patchesBySupervisorAppVersion, listOfMissingPatches[patchIndex], patchIndex,
                    listOfMissingPatches.Length);
            }
        }

        private string GetPathToPatchesDirectoryBySupervisorAppVersion()
        {
            var pathToRootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var patchesDirectory = this.fileSystemAccessor.CombinePath(pathToRootDirectory, "patches");
            if (!this.fileSystemAccessor.IsDirectoryExists(patchesDirectory))
                this.fileSystemAccessor.CreateDirectory(patchesDirectory);

            var sAppVersion = this.supervisorSettings.GetApplicationVersionCode().ToString();
            var patchesBySupervisorAppVersion = this.fileSystemAccessor.CombinePath(patchesDirectory, sAppVersion);

            if (!this.fileSystemAccessor.IsDirectoryExists(patchesBySupervisorAppVersion))
                this.fileSystemAccessor.CreateDirectory(patchesBySupervisorAppVersion);

            // remove old patches by old supervisor app version
            this.fileSystemAccessor.GetDirectoriesInDirectory(patchesDirectory)
                .Where(x => x != sAppVersion)
                .ForEach(x => this.fileSystemAccessor.DeleteDirectory(x));

            return patchesBySupervisorAppVersion;
        }

        private async Task DownloadPatchFileAsync(string patchesDirectory, InterviewerApplicationPatchApiView patchInfo,
            int patchIndex, int patchesCount)
        {
            var patchFilePath = this.fileSystemAccessor.CombinePath(patchesDirectory, patchInfo.FileName);

            Stopwatch sw = null;
            try
            {
                var patch = await this.synchronizationService.GetFileAsync(patchInfo.Url,
                    new Progress<TransferProgress>(downloadProgress =>
                    {
                        if (sw == null) sw = Stopwatch.StartNew();
                        if (downloadProgress.ProgressPercentage % 1 != 0) return;

                        var receivedKilobytes = downloadProgress.BytesReceived.Bytes();
                        var totalKilobytes = (downloadProgress.TotalBytesToReceive ?? 0).Bytes();

                        Context.Progress.Report(new SyncProgressInfo
                        {
                            Title = SupervisorUIResources.Synchronization_Download_Interviewer_App_Patches_Format
                                .FormatString(patchIndex + 1, patchesCount),
                            Description = string.Format(
                                InterviewerUIResources.Synchronization_DownloadApplication_Description,
                                receivedKilobytes.Humanize("00.00 MB"),
                                totalKilobytes.Humanize("00.00 MB"),
                                receivedKilobytes.Per(sw.Elapsed).Humanize("00.00"),
                                (int) downloadProgress.ProgressPercentage),
                            Status = SynchronizationStatus.Download
                        });
                    }), Context.CancellationToken);

                if (patch != null) this.fileSystemAccessor.WriteAllBytes(patchFilePath, patch);
            }
            catch (Exception exc)
            {
                this.logger.Error($"Error on downloading {patchInfo.FileName}", exc);
            }
        }
    }
}
