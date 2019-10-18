using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
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

    public class SendTabletInformationViewModel : MvxNotifyPropertyChanged
    {
        private readonly IBackupRestoreService backupRestoreService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger logger;

        private string scope;
        private DateTime whenGenerated;
        private bool isPackageBuild;
        private bool isPackageSendingAttemptCompleted;
        private string packageSendingAttemptResponseText;
        private bool isInProgress;
        private string informationPackageFilePath;

        public SendTabletInformationViewModel(
            IBackupRestoreService backupRestoreService,
            ILogger logger, 
            IUserInteractionService userInteractionService,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.backupRestoreService = backupRestoreService;
            this.logger = logger;
            this.userInteractionService = userInteractionService;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public string Scope
        {
            get => this.scope;
            set => this.RaiseAndSetIfChanged( ref this.scope, value);
        }

        public DateTime WhenGenerated
        {
            get => this.whenGenerated;
            set => this.RaiseAndSetIfChanged( ref this.whenGenerated, value);
        }

        public bool IsPackageBuild
        {
            get => this.isPackageBuild;
            set => this.RaiseAndSetIfChanged( ref this.isPackageBuild, value);
        }

        public bool IsPackageSendingAttemptCompleted
        {
            get => this.isPackageSendingAttemptCompleted;
            set => this.RaiseAndSetIfChanged( ref this.isPackageSendingAttemptCompleted, value);
        }

        public string PackageSendingAttemptResponceText
        {
            get => this.packageSendingAttemptResponseText;
            set => this.RaiseAndSetIfChanged( ref this.packageSendingAttemptResponseText, value);
        }

        public bool IsInProgress
        {
            get => this.isInProgress;
            set => this.RaiseAndSetIfChanged( ref this.isInProgress, value);
        }

        private async Task CreateBackupAsync()
        {
            if (this.IsInProgress)
                return;

            this.IsPackageBuild = false;
            this.IsPackageSendingAttemptCompleted = false;
            this.IsInProgress = true;

            try
            {
                var backupFilePath = await this.backupRestoreService.BackupAsync().ConfigureAwait(false);
                this.IsPackageBuild = true;

                var fileSize = this.fileSystemAccessor.GetFileSize(backupFilePath);
                this.Scope = FileSizeUtils.SizeSuffix(fileSize);

                this.WhenGenerated = DateTime.Now;
                this.informationPackageFilePath = backupFilePath;
            }
            catch (Exception e)
            {
                this.logger.Error(e.Message, e);
                await this.userInteractionService.AlertAsync(e.Message);
            }

            this.IsInProgress = false;
        }

        private async Task SendBackupAsync(CancellationToken cancellationToken)
        {
            if (this.IsInProgress)
                return;

            this.IsInProgress = true;

            try
            {
                await this.backupRestoreService.SendBackupAsync(informationPackageFilePath, cancellationToken);
                this.PackageSendingAttemptResponceText = EnumeratorUIResources.Troubleshooting_InformationPackageIsSuccessfullySent;
            }
            catch (SynchronizationException ex)
            {
                this.logger.Error("Error when sending backup. ", ex);
                this.PackageSendingAttemptResponceText = ex.Message;
            }
            this.IsPackageSendingAttemptCompleted = true;
            this.IsPackageBuild = false;
            this.IsInProgress = false;
        }

        private void DeleteBackup()
        {
            this.IsPackageBuild = false;
        }

        public IMvxAsyncCommand CreateBackupCommand => new MvxAsyncCommand(this.CreateBackupAsync);
        public IMvxAsyncCommand SendBackupCommand => new MvxAsyncCommand(this.SendBackupAsync);
        public IMvxCommand DeleteBackupCommand => new MvxCommand(this.DeleteBackup);
       
    }
}
