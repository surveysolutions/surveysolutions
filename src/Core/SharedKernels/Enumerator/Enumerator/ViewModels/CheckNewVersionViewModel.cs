using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class CheckNewVersionViewModel : MvxNotifyPropertyChanged
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IDeviceSettings deviceSettings;
        private readonly ITabletDiagnosticService tabletDiagnosticService;
        private readonly ILogger logger;

        private CancellationTokenSource cancellationTokenSource;

        private bool isVersionCheckInProgress;
        private bool isNewVersionAvailable;
        private string checkNewVersionResult;
        private string checkNewVersionDetails;

        public CheckNewVersionViewModel(
            ISynchronizationService synchronizationService, 
            IDeviceSettings deviceSettings, 
            ITabletDiagnosticService tabletDiagnosticService, 
            ILogger logger)
        {
            this.synchronizationService = synchronizationService;
            this.deviceSettings = deviceSettings;
            this.tabletDiagnosticService = tabletDiagnosticService;
            this.logger = logger;
            this.Version = this.deviceSettings.GetApplicationVersionName();
            this.cancellationTokenSource = new CancellationTokenSource(this.downloadApkTimeout);
        }

        public bool IsVersionCheckInProgress
        {
            get => this.isVersionCheckInProgress;
            set => this.RaiseAndSetIfChanged( ref this.isVersionCheckInProgress, value);
        }

        public bool IsNewVersionAvailable
        {
            get => this.isNewVersionAvailable;
            set => this.RaiseAndSetIfChanged( ref this.isNewVersionAvailable, value);
        }

        public string CheckNewVersionResult
        {
            get => this.checkNewVersionResult;
            set => this.RaiseAndSetIfChanged( ref this.checkNewVersionResult, value);
        }

        public string CheckNewVersionDetails
        {
            get => this.checkNewVersionDetails;
            set => this.RaiseAndSetIfChanged( ref this.checkNewVersionDetails, value);
        }

        public string Version { get; set; }

        public IMvxAsyncCommand CheckVersionCommand => new MvxAsyncCommand(this.CheckVersion);

        public IMvxCommand CancelUpgrade => new MvxCommand(() => this.cancellationTokenSource.Cancel());
        public IMvxAsyncCommand UpdateApplicationCommand => new MvxAsyncCommand(this.UpdateApplication);
        public IMvxCommand RejectUpdateApplicationCommand => new MvxCommand(RejectUpdateApplication);

        private readonly TimeSpan downloadApkTimeout = TimeSpan.FromMinutes(30);
        
        private async Task UpdateApplication()
        {
            this.IsNewVersionAvailable = false;
            this.IsVersionCheckInProgress = true;
            this.cancellationTokenSource = new CancellationTokenSource(this.downloadApkTimeout);
            this.CheckNewVersionDetails = string.Empty;
            try
            {
                this.CheckNewVersionResult = EnumeratorUIResources.Diagnostics_DownloadingPleaseWait;

                await this.tabletDiagnosticService.UpdateTheApp(this.cancellationTokenSource.Token, true,
                    new Progress<TransferProgress>(progress =>
                    {
                        this.CheckNewVersionResult = EnumeratorUIResources.Diagnostics_DownloadingPleaseWait
                                                     + $" ({(int)progress.ProgressPercentage}%)";

                        var bytesReceived = NumericTextFormatter.FormatBytesHumanized(progress.BytesReceived);
                        var total = progress.TotalBytesToReceive.HasValue
                            ? NumericTextFormatter.FormatBytesHumanized(progress.TotalBytesToReceive.Value)
                            : "N/A";

                        var speed = progress.Speed.HasValue
                            ? NumericTextFormatter.FormatSpeedHumanized(progress.Speed.Value, TimeSpan.FromSeconds(1))
                            : "N/A";

                        this.CheckNewVersionDetails = $"{bytesReceived}/{total} @ {speed}";
                    }));

                this.CheckNewVersionResult = null;
                this.checkNewVersionDetails = string.Empty;
            }
            catch (Exception) when (cancellationTokenSource.IsCancellationRequested)
            {
                this.CheckNewVersionResult = EnumeratorUIResources.RequestCanceledByUser;
            }
            catch (Exception ex)
            {
                this.logger.Error("Error when updating", ex);
                this.CheckNewVersionResult = ex.Message;
                this.CheckNewVersionDetails = string.Empty;
            }

            this.checkNewVersionDetails = string.Empty;
            this.IsVersionCheckInProgress = false;
        }

        private async Task CheckVersion()
        {
            this.IsNewVersionAvailable = false;
            this.CheckNewVersionResult = null;
            this.IsVersionCheckInProgress = true;
            this.cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var versionFromServer = 
                    await this.synchronizationService.GetLatestApplicationVersionAsync(this.cancellationTokenSource.Token);

                if (!this.cancellationTokenSource.IsCancellationRequested)
                {
                    if (versionFromServer.HasValue && versionFromServer > this.deviceSettings.GetApplicationVersionCode())
                    {
                        this.IsNewVersionAvailable = true;
                    }
                    else
                    {
                        this.CheckNewVersionResult =
                            EnumeratorUIResources.Diagnostics_YouHaveTheLatestVersionOfApplication;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error("Error when checking for update", ex);
                this.CheckNewVersionResult = ex.Message;
            }

            this.IsVersionCheckInProgress = false;
        }

        private void RejectUpdateApplication()
        {
            this.IsNewVersionAvailable = false;
        }
    }
}
