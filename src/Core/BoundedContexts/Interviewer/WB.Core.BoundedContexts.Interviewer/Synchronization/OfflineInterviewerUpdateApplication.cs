using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Humanizer;
using Plugin.Permissions.Abstractions;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Synchronization
{
    public class OfflineInterviewerUpdateApplication : SynchronizationStep
    {
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPermissionsService permissions;
        private readonly IViewModelNavigationService navigationService;
        private readonly IPathUtils pathUtils;

        public OfflineInterviewerUpdateApplication(int sortOrder, 
            ISynchronizationService synchronizationService,
            ILogger logger, 
            IInterviewerSettings interviewerSettings,
            IFileSystemAccessor fileSystemAccessor,
            IPermissionsService permissions,
            IViewModelNavigationService navigationService,
            IPathUtils pathUtils) : base(sortOrder, synchronizationService, logger)
        {
            this.interviewerSettings = interviewerSettings ?? throw new ArgumentNullException(nameof(interviewerSettings));
            this.fileSystemAccessor = fileSystemAccessor;
            this.permissions = permissions;
            this.navigationService = navigationService;
            this.pathUtils = pathUtils;
        }


        public override async Task ExecuteAsync()
        {
            if (this.interviewerSettings.AllowSyncWithHq) return;

            Context.Progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_CheckNewVersionOfApplication,
                Status = SynchronizationStatus.Started,
                Stage = SyncStage.CheckNewVersionOfApplication
            });

            var versionFromServer = await
                this.synchronizationService.GetLatestApplicationVersionAsync(Context.CancellationToken);

            Context.CancellationToken.ThrowIfCancellationRequested();

            if (versionFromServer.HasValue && versionFromServer > interviewerSettings.GetApplicationVersionCode())
            {
                await this.permissions.AssureHasPermission(Permission.Storage);

                try
                {
                    var apkBytes = await this.synchronizationService.GetApplicationAsync(
                        Context.CancellationToken, new Progress<TransferProgress>(downloadProgress =>
                        {

                            var receivedKilobytes = downloadProgress.BytesReceived.Bytes();
                            var totalKilobytes = (downloadProgress.TotalBytesToReceive ?? 0).Bytes();
                            var speed = (downloadProgress.Speed ?? 0).Bytes().Per(TimeSpan.FromSeconds(1)).Humanize("0.00");

                            Context.Progress.Report(new SyncProgressInfo
                            {
                                Title = InterviewerUIResources.Synchronization_DownloadApplication,
                                Description = string.Format(
                                    InterviewerUIResources.Synchronization_DownloadApplication_Description,
                                    receivedKilobytes.Humanize("0.00"),
                                    totalKilobytes.Humanize("0.00"),
                                    speed,
                                    (int)downloadProgress.ProgressPercentage),
                                Status = SynchronizationStatus.Download,
                                Stage = SyncStage.DownloadApplication,
                                StageExtraInfo = new Dictionary<string, string>()
                                {
                                    { "receivedKilobytes", receivedKilobytes.Humanize("0.00") },
                                    { "totalKilobytes", totalKilobytes.Humanize("0.00")},
                                    { "receivingRate", speed},
                                    {"progressPercentage",((int) downloadProgress.ProgressPercentage).ToString()}
                                }
                            });
                        }));

                    Context.CancellationToken.ThrowIfCancellationRequested();

                    if (apkBytes != null)
                    {
                        var pathToNewApk = this.fileSystemAccessor.CombinePath(
                            this.pathUtils.GetRootDirectory(), "interviewer.apk");

                        this.fileSystemAccessor.WriteAllBytes(pathToNewApk, apkBytes);

                        this.navigationService.InstallNewApp(pathToNewApk);
                        this.navigationService.CloseApplication();
                    }
                }
                catch (SynchronizationException ex) when (ex.InnerException is RestException rest)
                {
                    if (rest.StatusCode != HttpStatusCode.NotFound)
                        throw;
                }
                catch (Exception ex)
                {
                    this.logger.Error("Could not download apk and update application", ex);
                }
            }
        }
    }
}
