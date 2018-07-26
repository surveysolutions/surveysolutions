using System.Threading;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class LocalSynchronizationViewModel : SynchronizationViewModelBase
    {
        private readonly IMvxMessenger messenger;
        private readonly ISynchronizationCompleteSource synchronizationCompleteSource;

        public LocalSynchronizationViewModel(IMvxMessenger messenger, ISynchronizationCompleteSource synchronizationCompleteSource)
        {
            this.messenger = messenger;
            this.synchronizationCompleteSource = synchronizationCompleteSource;
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

        public IMvxCommand CancelSynchronizationCommand => new MvxCommand(this.CancelSynchronizaion);
        public IMvxCommand HideSynchronizationCommand => new MvxCommand(this.HideSynchronizaion);

        public void HideSynchronizaion()
        {
            this.IsSynchronizationInfoShowed = false;
        }

        public void CancelSynchronizaion()
        {
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
