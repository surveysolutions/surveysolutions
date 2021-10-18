using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class LocalSynchronizationViewModel : SynchronizationViewModelBase
    {
        private readonly IMvxMessenger messenger;
        private readonly ISynchronizationCompleteSource synchronizationCompleteSource;
        private readonly ITabletDiagnosticService diagnosticService;
        private readonly ILogger logger;

        public LocalSynchronizationViewModel( 
            ISynchronizationCompleteSource synchronizationCompleteSource,
            ITabletDiagnosticService diagnosticService,
            ILogger logger)
        {
            this.messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
            this.synchronizationCompleteSource = synchronizationCompleteSource;
            this.diagnosticService = diagnosticService;
            this.logger = logger;
        }

        public void Init()
        {
            var syncProgressDto = this.SyncBgService?.CurrentProgress;
            if (syncProgressDto != null)
            {
                syncProgressDto.Progress.ProgressChanged += ProgressOnProgressChanged;
                this.synchronizationCancellationTokenSource = syncProgressDto.CancellationTokenSource;
            }
        }

        public ISyncBgService<SyncProgressDto> SyncBgService { get; set; }        

        private CancellationTokenSource synchronizationCancellationTokenSource;

        public IMvxAsyncCommand UpdateApplicationCommand => new MvxAsyncCommand(UpdateApplication);

        private async Task UpdateApplication()
        {
            this.IsSynchronizationInfoShowed = true;
            this.IsUpdateRequired = false;
            this.SynchronizationErrorOccured = false;
            this.Status = SynchronizationStatus.Download;

            this.synchronizationCancellationTokenSource = new CancellationTokenSource();
            try
            {
                this.ProcessOperation = EnumeratorUIResources.Diagnostics_DownloadingPleaseWait;

                await this.diagnosticService.UpdateTheApp(synchronizationCancellationTokenSource.Token, true,
                    new Progress<TransferProgress>(progress =>
                    {
                        this.ProcessOperation = EnumeratorUIResources.Diagnostics_DownloadingPleaseWait
                                                     + $" ({(int)progress.ProgressPercentage}%)";
                    }));

                this.Status = SynchronizationStatus.Success;
            }
            catch (Exception) when (synchronizationCancellationTokenSource.IsCancellationRequested)
            {
                this.ProcessOperation = EnumeratorUIResources.RequestCanceledByUser;
                this.Status = SynchronizationStatus.Canceled;
            }
            catch (Exception ex)
            {
                this.ProcessOperation = EnumeratorUIResources.UpgradeRequired;
                this.SynchronizationErrorOccured = true;
                this.Status = SynchronizationStatus.Fail;
                this.logger.Fatal("Failed to update app", ex);
            }
        }

        public IMvxCommand CancelSynchronizationCommand => new MvxCommand(this.CancelSynchronizaion);
        public IMvxCommand HideSynchronizationCommand => new MvxCommand(this.HideSynchronizaion);

        public void HideSynchronizaion()
        {
            this.IsSynchronizationInfoShowed = false;
        }

        public event EventHandler OnCancel;

        public void CancelSynchronizaion()
        {
            OnCancel?.Invoke(this, EventArgs.Empty);
            

            this.IsSynchronizationInfoShowed = false;
            this.IsSynchronizationInProgress = false;
            if (this.synchronizationCancellationTokenSource != null && !this.synchronizationCancellationTokenSource.IsCancellationRequested)
                this.synchronizationCancellationTokenSource.Cancel();
        }

        public virtual void Synchronize()
        {
            this.IsSynchronizationInfoShowed = true;
            this.synchronizationCancellationTokenSource = new CancellationTokenSource();

            this.SynchronizationErrorOccured = false;
            this.SyncBgService.StartSync();
            var syncProgressDto = this.SyncBgService.CurrentProgress;
            if (syncProgressDto != null)
            {
                syncProgressDto.Progress.ProgressChanged += ProgressOnProgressChanged;
                this.synchronizationCancellationTokenSource = syncProgressDto.CancellationTokenSource;
            }
        }

        protected override void OnSyncCompleted()
        {
            this.messenger.Publish(new DashboardChangedMsg(this));
            synchronizationCompleteSource.NotifyOnCompletedSynchronization(true);
        }
    }
}
