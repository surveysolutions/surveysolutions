using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class SendLogsViewModel : MvxNotifyPropertyChanged
    {
        private bool isInProgress;
        private readonly IBackupRestoreService backupRestoreService;
        private bool logsSent;

        public SendLogsViewModel(IBackupRestoreService backupRestoreService)
        {
            this.backupRestoreService = backupRestoreService;
        }

        public IMvxAsyncCommand SendLogsCommand => new MvxAsyncCommand(this.SendLogsAsync, () => !this.IsInProgress);

        private async Task SendLogsAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.LogsSent = false;
                this.IsInProgress = true;
                await this.backupRestoreService.SendLogsAsync(cancellationToken);
                this.LogsSent = true;
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        public bool IsInProgress
        {
            get => this.isInProgress;
            set => this.SetProperty(ref this.isInProgress, value);
        }

        public bool LogsSent
        {
            get => logsSent;
            set => this.SetProperty(ref this.logsSent, value);
        }
    }
}
