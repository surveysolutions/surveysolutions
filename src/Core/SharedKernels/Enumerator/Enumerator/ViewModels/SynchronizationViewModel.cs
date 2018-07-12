using System;
using System.Threading;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class SynchronizationViewModel : MvxNotifyPropertyChanged
    {
        private readonly IMvxMessenger messenger;
        private readonly ISynchronizationCompleteSource synchronizationCompleteSource;

        public SynchronizationViewModel(IMvxMessenger messenger, ISynchronizationCompleteSource synchronizationCompleteSource)
        {
            this.messenger = messenger;
            this.synchronizationCompleteSource = synchronizationCompleteSource;
        }

        public ISyncBgService<SyncProgressDto> SyncBgService { get; set; }


        private bool hasUserAnotherDevice;

        private bool isSynchronizationInfoShowed;
        private bool isSynchronizationInProgress;
        private string processOperation;
        private string processOperationDescription;
        private SynchronizationStatistics statistics;
        private SynchronizationStatus status;
        private CancellationTokenSource synchronizationCancellationTokenSource;
        private bool synchronizationErrorOccured;

        public SynchronizationStatistics Statistics
        {
            get => statistics;
            set => this.RaiseAndSetIfChanged(ref this.statistics, value);
        }

        public SynchronizationStatus Status
        {
            get => this.status;
            set => this.RaiseAndSetIfChanged( ref this.status, value);
        }

        public bool SynchronizationErrorOccured
        {
            get => this.synchronizationErrorOccured;
            set => this.RaiseAndSetIfChanged( ref this.synchronizationErrorOccured, value);
        }

        public bool IsSynchronizationInfoShowed
        {
            get => this.isSynchronizationInfoShowed;
            set => this.RaiseAndSetIfChanged( ref this.isSynchronizationInfoShowed, value);
        }

        public bool IsSynchronizationInProgress
        {
            get => this.isSynchronizationInProgress;
            set => this.RaiseAndSetIfChanged( ref this.isSynchronizationInProgress, value);
        }

        public bool HasUserAnotherDevice
        {
            get => this.hasUserAnotherDevice;
            set => this.RaiseAndSetIfChanged( ref this.hasUserAnotherDevice, value);
        }

        public string ProcessOperation
        {
            get => this.processOperation;
            set => this.RaiseAndSetIfChanged( ref this.processOperation, value);
        }

        public string ProcessOperationDescription
        {
            get => this.processOperationDescription;
            set => this.RaiseAndSetIfChanged( ref this.processOperationDescription, value);
        }

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

        private void ProgressOnProgressChanged(object sender, SyncProgressInfo syncProgressInfo)
        {
            this.InvokeOnMainThread(() =>
            {
                this.IsSynchronizationInProgress = syncProgressInfo.IsRunning;
                this.ProcessOperation = syncProgressInfo.Title;
                this.ProcessOperationDescription = syncProgressInfo.Description;
                this.Statistics = syncProgressInfo.Statistics;

                this.Status = syncProgressInfo.Status;
                this.SynchronizationErrorOccured = this.SynchronizationErrorOccured || syncProgressInfo.HasErrors;

                this.HasUserAnotherDevice = syncProgressInfo.UserIsLinkedToAnotherDevice;

                if (!syncProgressInfo.IsRunning)
                {
                    this.OnSyncCompleted();
                }
            });
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

        protected virtual void OnSyncCompleted()
        {
            this.messenger.Publish(new DashboardChangedMsg(this));
            synchronizationCompleteSource.NotifyOnCompletedSynchronization(true);
        }
    }
}
