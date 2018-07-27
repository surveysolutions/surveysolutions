using System;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class SynchronizationViewModelBase : MvxNotifyPropertyChanged
    {
        private bool isSynchronizationInfoShowed;
        private bool isSynchronizationInProgress;
        private string processOperation;
        private string processOperationDescription;
        private SynchronizationStatistics statistics;
        private SynchronizationStatus status;
        private bool synchronizationErrorOccured;
        private bool hasUserAnotherDevice;

        public SynchronizationStatistics Statistics
        {
            get => statistics;
            set => SetProperty(ref this.statistics, value);
        }

        public SynchronizationStatus Status
        {
            get => this.status;
            set => SetProperty(ref this.status, value);
        }

        public bool SynchronizationErrorOccured
        {
            get => this.synchronizationErrorOccured;
            set => SetProperty( ref this.synchronizationErrorOccured, value);
        }

        public virtual bool IsSynchronizationInfoShowed
        {
            get => this.isSynchronizationInfoShowed;
            set => SetProperty( ref this.isSynchronizationInfoShowed, value);
        }

        public bool IsSynchronizationInProgress
        {
            get => this.isSynchronizationInProgress;
            set => SetProperty( ref this.isSynchronizationInProgress, value);
        }

        public virtual bool HasUserAnotherDevice
        {
            get => this.hasUserAnotherDevice;
            set => SetProperty( ref this.hasUserAnotherDevice, value);
        }

        public string ProcessOperation
        {
            get => this.processOperation;
            set => SetProperty( ref this.processOperation, value);
        }

        public virtual bool CanBeManaged => true;

        public string ProcessOperationDescription
        {
            get => this.processOperationDescription;
            set => SetProperty( ref this.processOperationDescription, value);
        }

        public void ProgressOnProgressChanged(object sender, SyncProgressInfo syncProgressInfo)
        {
            this.InvokeOnMainThread(() =>
            {
                if (syncProgressInfo.TransferProgress == null)
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
                }

                OnProgressChanged?.Invoke(this, syncProgressInfo);
            });
        }

        public event EventHandler<SyncProgressInfo> OnProgressChanged;

        protected abstract void OnSyncCompleted();
    }
}
