using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Humanizer;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public abstract class UpdateApplication : SynchronizationStep
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly ITabletDiagnosticService diagnosticService;

        public UpdateApplication(int sortOrder,
            ISynchronizationService synchronizationService,
            ITabletDiagnosticService diagnosticService,
            ILogger logger) : base(sortOrder, synchronizationService, logger)
        {
            this.synchronizationService = synchronizationService;
            this.diagnosticService = diagnosticService;
        }

        public override async Task ExecuteAsync()
        {
             if (!await this.synchronizationService.IsAutoUpdateEnabledAsync(Context.CancellationToken))
                return;

            Context.Progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_CheckNewVersionOfApplication,
                Status = SynchronizationStatus.Started,
                Stage= SyncStage.CheckNewVersionOfApplication
            });

            var versionFromServer = await
                this.synchronizationService.GetLatestApplicationVersionAsync(Context.CancellationToken);

            if (versionFromServer.HasValue && versionFromServer > GetApplicationVersionCode())
            {
                Stopwatch sw = null;
                try
                {
                    await this.diagnosticService.UpdateTheApp(Context.CancellationToken, false, new Progress<TransferProgress>(downloadProgress =>
                    {
                        if (sw == null) sw = Stopwatch.StartNew();
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
                                receivedKilobytes.Per(sw.Elapsed).Humanize("00.00"),
                                (int) downloadProgress.ProgressPercentage),
                            Status = SynchronizationStatus.Download,
                            Stage = SyncStage.DownloadApplication,
                            StageExtraInfo = new Dictionary<string, string>()
                            {
                                { "receivedKilobytes", receivedKilobytes.Humanize("00.00 MB") },
                                { "totalKilobytes", totalKilobytes.Humanize("00.00 MB")},
                                { "receivingRate", receivedKilobytes.Per(sw.Elapsed).Humanize("00.00")},
                                {"progressPercentage",((int) downloadProgress.ProgressPercentage).ToString()}
                            }
                        });
                    }));
                }
                catch (Exception exc)
                {
                    this.logger.Error("Error on auto updating", exc);
                }
            }
        }

        protected abstract int GetApplicationVersionCode();
    }
}
