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
        private readonly ISupervisorSynchronizationService supervisorSynchronization;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPermissionsService permissions;

        public DownloadInterviewerAppPatches(int sortOrder,
            ISynchronizationService synchronizationService,
            ILogger logger,
            ISupervisorSynchronizationService supervisorSynchronization,
            IFileSystemAccessor fileSystemAccessor,
            IPermissionsService permissions) :
            base(sortOrder, synchronizationService, logger)
        {
            this.supervisorSynchronization = supervisorSynchronization;
            this.fileSystemAccessor = fileSystemAccessor;
            this.permissions = permissions;
        }

        public override async Task ExecuteAsync()
        {
            Context.Progress.Report(new SyncProgressInfo
            {
                Title = SupervisorUIResources.Synchronization_Download_Interviewer_App_Patches,
                Status = SynchronizationStatus.Download
            });

            await this.permissions.AssureHasPermission(Permission.Storage);

            var pathToRootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var patchesDirectory = this.fileSystemAccessor.CombinePath(pathToRootDirectory, "patches");
            if (!this.fileSystemAccessor.IsDirectoryExists(patchesDirectory))
                this.fileSystemAccessor.CreateDirectory(patchesDirectory);
            
            var listOfPatchesInfo = await this.supervisorSynchronization.GetListOInterviewerAppPatchesAsync(Context.CancellationToken);
            var listOfMissingPatches = listOfPatchesInfo
                .Where(x => !this.fileSystemAccessor.IsFileExists(
                    this.fileSystemAccessor.CombinePath(patchesDirectory, x.FileName)))
                .ToArray();

            for(int patchIndex = 0; patchIndex < listOfMissingPatches.Length; patchIndex++)
            {
                var patchInfo = listOfMissingPatches[patchIndex];
                var patchFilePath = this.fileSystemAccessor.CombinePath(patchesDirectory, patchInfo.FileName);

                Stopwatch sw = null;
                try
                {
                    var patch = await this.supervisorSynchronization.GetFileAsync(patchInfo.Url,
                        new Progress<TransferProgress>(downloadProgress =>
                        {
                            if (sw == null) sw = Stopwatch.StartNew();
                            if (downloadProgress.ProgressPercentage % 1 != 0) return;

                            var receivedKilobytes = downloadProgress.BytesReceived.Bytes();
                            var totalKilobytes = (downloadProgress.TotalBytesToReceive ?? 0).Bytes();

                            Context.Progress.Report(new SyncProgressInfo
                            {
                                Title = SupervisorUIResources.Synchronization_Download_Interviewer_App_Patches_Format
                                    .FormatString(patchIndex + 1, listOfMissingPatches.Length),
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
}
