using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Synchronization;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class SynchronizationViewModel : MvxNotifyPropertyChanged
    {
        private readonly SynchronizationProcessor synchronizationProcessor;
        private readonly CancellationTokenSource synchronizationCancellationTokenSource = new CancellationTokenSource();

        public SynchronizationViewModel(SynchronizationProcessor synchronizationProcessor)
        {
            this.synchronizationProcessor = synchronizationProcessor;
        }

        private bool isSynchronizationInProgress;
        public bool IsSynchronizationInProgress
        {
            get { return this.isSynchronizationInProgress; }
            set { this.isSynchronizationInProgress = value; this.RaisePropertyChanged(); }
        }

        public async Task SynchronizeAsync()
        {
            this.IsSynchronizationInProgress = true;
            try
            {
                await this.synchronizationProcessor.Run(this.synchronizationCancellationTokenSource.Token);
            }
            finally
            {
                this.IsSynchronizationInProgress = false;
            }
        }

        public void CancelSynchronizaion()
        {
            this.synchronizationCancellationTokenSource.Cancel();
        }
    }
}