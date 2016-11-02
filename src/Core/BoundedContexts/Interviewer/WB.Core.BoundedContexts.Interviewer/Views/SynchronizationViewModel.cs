using System;
using System.Threading;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class SynchronizationViewModel : MvxNotifyPropertyChanged
    {
        public ISyncBgService SyncBgService { get; set; }

        public event EventHandler SyncCompleted;

        private bool hasUserAnotherDevice;

        private bool isSynchronizationInfoShowed;
        private bool isSynchronizationInProgress;
        private string processOperation;
        private string processOperationDescription;
        private SychronizationStatistics statistics;
        private SynchronizationStatus status;
        private CancellationTokenSource synchronizationCancellationTokenSource;
        private bool synchronizationErrorOccured;

        public SychronizationStatistics Statistics
        {
            get { return statistics; }
            set
            {
                statistics = value;
                this.RaisePropertyChanged();
            }
        }

        public SynchronizationStatus Status
        {
            get { return this.status; }
            set
            {
                this.status = value;
                this.RaisePropertyChanged();
            }
        }

        public bool SynchronizationErrorOccured
        {
            get { return this.synchronizationErrorOccured; }
            set
            {
                this.synchronizationErrorOccured = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsSynchronizationInfoShowed
        {
            get { return this.isSynchronizationInfoShowed; }
            set
            {
                this.isSynchronizationInfoShowed = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsSynchronizationInProgress
        {
            get { return this.isSynchronizationInProgress; }
            set
            {
                this.isSynchronizationInProgress = value;
                this.RaisePropertyChanged();
            }
        }

        public bool HasUserAnotherDevice
        {
            get { return this.hasUserAnotherDevice; }
            set
            {
                this.hasUserAnotherDevice = value;
                this.RaisePropertyChanged();
            }
        }

        public string ProcessOperation
        {
            get { return this.processOperation; }
            set
            {
                if (this.processOperation == value) return;

                this.processOperation = value;
                this.RaisePropertyChanged();
            }
        }

        public string ProcessOperationDescription
        {
            get { return this.processOperationDescription; }
            set
            {
                this.processOperationDescription = value;
                this.RaisePropertyChanged();
            }
        }

        public IMvxCommand CancelSynchronizationCommand => new MvxCommand(this.CancelSynchronizaion);

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
                this.SynchronizationErrorOccured = SynchronizationErrorOccured || syncProgressInfo.HasErrors;

                this.IsSynchronizationInProgress = syncProgressInfo.IsRunning;
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
            this.SyncCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}