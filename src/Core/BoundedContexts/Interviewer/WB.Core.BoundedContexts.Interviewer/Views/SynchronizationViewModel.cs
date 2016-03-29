using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class SynchronizationViewModel : MvxNotifyPropertyChanged
    {
        private readonly IMvxMessenger messenger;
        private readonly ISynchronizationProcess synchronizationProcess;

        private bool hasUserAnotherDevice;

        private bool isSynchronizationInfoShowed;

        private bool isSynchronizationInProgress;

        private string processOperation;

        public string processOperationDescription;

        private SychronizationStatistics statistics;

        private SynchronizationStatus status;

        private CancellationTokenSource synchronizationCancellationTokenSource;

        public SynchronizationViewModel(IMvxMessenger messenger,
            ISynchronizationProcess synchronizationProcess)
        {
            this.messenger = messenger;
            this.synchronizationProcess = synchronizationProcess;
        }

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
            if (this.synchronizationCancellationTokenSource != null &&
                !this.synchronizationCancellationTokenSource.IsCancellationRequested)
                this.synchronizationCancellationTokenSource.Cancel();
        }

        private void ProgressOnProgressChanged(object sender, SyncProgressInfo syncProgressInfo)
        {
            this.InvokeOnMainThread(() =>
            {
                this.ProcessOperation = syncProgressInfo.Title;
                this.ProcessOperationDescription = syncProgressInfo.Description;
                this.Statistics = syncProgressInfo.Statistics;
                this.Status = syncProgressInfo.Status;
            });
        }

        public async Task SynchronizeAsync()
        {
            try
            {
                this.messenger.Publish(new SyncronizationStartedMessage(this));
                this.IsSynchronizationInfoShowed = true;
                this.IsSynchronizationInProgress = true;
                this.synchronizationCancellationTokenSource = new CancellationTokenSource();
                var progress = new Progress<SyncProgressInfo>();
                progress.ProgressChanged += ProgressOnProgressChanged;
                await
                    this.synchronizationProcess.SyncronizeAsync(progress,
                        this.synchronizationCancellationTokenSource.Token);
            }
            finally
            {
                this.IsSynchronizationInProgress = false;
                this.messenger.Publish(new SyncronizationStoppedMessage(this));
            }
        }
    }

    public class SyncronizationStoppedMessage : MvxMessage
    {
        public SyncronizationStoppedMessage(object sender) : base(sender)
        {
        }
    }

    public class SyncronizationStartedMessage : MvxMessage
    {
        public SyncronizationStartedMessage(object sender) : base(sender)
        {
        }
    }
}