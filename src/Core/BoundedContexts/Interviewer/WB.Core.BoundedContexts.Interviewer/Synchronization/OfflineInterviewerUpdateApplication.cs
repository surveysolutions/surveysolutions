using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Humanizer;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using Xamarin.Essentials;

namespace WB.Core.BoundedContexts.Interviewer.Synchronization
{
    public class OfflineInterviewerUpdateApplication : SynchronizationStep
    {
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IViewModelNavigationService navigationService;
        private readonly IPathUtils pathUtils;

        public OfflineInterviewerUpdateApplication(int sortOrder, 
            ISynchronizationService synchronizationService,
            ILogger logger, 
            IInterviewerSettings interviewerSettings,
            IFileSystemAccessor fileSystemAccessor,
            IViewModelNavigationService navigationService,
            IPathUtils pathUtils) : base(sortOrder, synchronizationService, logger)
        {
            this.interviewerSettings = interviewerSettings ?? throw new ArgumentNullException(nameof(interviewerSettings));
            this.fileSystemAccessor = fileSystemAccessor;
            this.navigationService = navigationService;
            this.pathUtils = pathUtils;
        }


        public override async Task ExecuteAsync()
        {
            if (this.interviewerSettings.AllowSyncWithHq) return;

            Context.Progress.Report(new SyncProgressInfo
            {
                Title = EnumeratorUIResources.Synchronization_CheckNewVersionOfApplication,
                Status = SynchronizationStatus.Started,
                Stage = SyncStage.CheckNewVersionOfApplication
            });

            var versionFromServer = await
                this.synchronizationService.GetLatestApplicationVersionAsync(Context.CancellationToken);

            Context.CancellationToken.ThrowIfCancellationRequested();

            if (versionFromServer.HasValue && versionFromServer > interviewerSettings.GetApplicationVersionCode())
            {
                try
                {
                    var apkBytes = await this.synchronizationService.GetApplicationAsync(
                        new Progress<TransferProgress>(downloadProgress =>
                        {

                            var receivedKilobytes = downloadProgress.BytesReceived.Bytes();
                            var totalKilobytes = (downloadProgress.TotalBytesToReceive ?? 0).Bytes();
                            var speed = (downloadProgress.Speed ?? 0).Bytes().Per(TimeSpan.FromSeconds(1)).Humanize("0.00");

                            Context.Progress.Report(new SyncProgressInfo
                            {
                                Title = EnumeratorUIResources.Synchronization_DownloadApplication,
                                Description = string.Format(
                                    EnumeratorUIResources.Synchronization_DownloadApplication_Description,
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
                        }), Context.CancellationToken);

                    Context.CancellationToken.ThrowIfCancellationRequested();

                    if (apkBytes != null)
                    {
                        var rootDirectory = await this.pathUtils.GetRootDirectoryAsync();
                        var pathToNewApk = this.fileSystemAccessor.CombinePath(
                            rootDirectory, "interviewer.apk");

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
