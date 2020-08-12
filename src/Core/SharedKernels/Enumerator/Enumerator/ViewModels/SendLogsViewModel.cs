using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class SendLogsViewModel : MvxNotifyPropertyChanged
    {
        private bool isInProgress;
        private readonly IBackupRestoreService backupRestoreService;
        private readonly ILogger logger;
        private bool logsSent;

        public SendLogsViewModel(IBackupRestoreService backupRestoreService, ILogger logger)
        {
            this.backupRestoreService = backupRestoreService;
            this.logger = logger;
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
            catch (SynchronizationException ex)
            {
                this.logger.Error("Error when sending logs. ", ex);
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
