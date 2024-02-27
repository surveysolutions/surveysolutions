using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public abstract class UpdateApplication : SynchronizationStep, IUpdateApplicationSynchronizationStep
    {
        private readonly ITabletDiagnosticService diagnosticService;

        protected UpdateApplication(int sortOrder,
            ISynchronizationService synchronizationService,
            ITabletDiagnosticService diagnosticService,
            ILogger logger) : base(sortOrder, synchronizationService, logger)
        {
            this.diagnosticService = diagnosticService;
        }

        public override async Task ExecuteAsync()
        {
            if (!await this.synchronizationService.IsAutoUpdateEnabledAsync(Context.CancellationToken).ConfigureAwait(false))
            {
                var versionFromServerCheck = await 
                    this.synchronizationService.GetLatestApplicationVersionAsync(Context.CancellationToken).ConfigureAwait(false);
                Context.Statistics.NewVersionExists = 
                    versionFromServerCheck.HasValue && versionFromServerCheck > GetApplicationVersionCode();

                return;
            }

            Context.Progress.Report(new SyncProgressInfo
            {
                Title = EnumeratorUIResources.Synchronization_CheckNewVersionOfApplication,
                Status = SynchronizationStatus.Started,
                Stage = SyncStage.CheckNewVersionOfApplication
            });

            var versionFromServer = await
                this.synchronizationService.GetLatestApplicationVersionAsync(Context.CancellationToken).ConfigureAwait(false);

            if (versionFromServer.HasValue && versionFromServer > GetApplicationVersionCode())
            {
                Stopwatch sw = null;
                try
                {
                    await this.diagnosticService.UpdateTheApp(Context.CancellationToken, false, new Progress<TransferProgress>(downloadProgress =>
                    {
                        if (sw == null) sw = Stopwatch.StartNew();
                        if (downloadProgress.ProgressPercentage % 1 != 0) return;

                        var receivedDataHumanized = NumericTextFormatter.FormatBytesHumanized(downloadProgress.BytesReceived);
                        var receivedSpeedHumanized = NumericTextFormatter.FormatSpeedHumanized(downloadProgress.BytesReceived, sw.Elapsed);
                        var totalSizeHumanized = NumericTextFormatter.FormatBytesHumanized(downloadProgress.TotalBytesToReceive ?? 0);

                        Context.Progress.Report(new SyncProgressInfo
                        {
                            Title = EnumeratorUIResources.Synchronization_DownloadApplication,
                            Description = string.Format(
                                EnumeratorUIResources.Synchronization_DownloadApplication_Description,
                                receivedDataHumanized,
                                totalSizeHumanized,
                                receivedSpeedHumanized,
                                (int) downloadProgress.ProgressPercentage),
                            Status = SynchronizationStatus.Download,
                            Stage = SyncStage.DownloadApplication,
                            StageExtraInfo = new Dictionary<string, string>()
                            {
                                { "receivedKilobytes", receivedDataHumanized },
                                { "totalKilobytes", totalSizeHumanized},
                                { "receivingRate", receivedSpeedHumanized},
                                { "progressPercentage", ((int) downloadProgress.ProgressPercentage).ToString()}
                            }
                        });
                    })).ConfigureAwait(false);
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
