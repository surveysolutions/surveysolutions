using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class BackupViewModel : MvxNotifyPropertyChanged
    {
        private readonly IBackupRestoreService backupRestoreService;
        private readonly ISynchronizationService synchronizationService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger logger;

        private string scope;
        private DateTime whenGenerated;
        private bool isPackageBuild;
        private bool isPackageSendingAttemptCompleted;
        private string packageSendingAttemptResponceText;
        private bool isInProgress;
        private string informationPackageFilePath;

        public BackupViewModel(
            IBackupRestoreService backupRestoreService,
            ISynchronizationService synchronizationService,
            ILogger logger, 
            IUserInteractionService userInteractionService,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.backupRestoreService = backupRestoreService;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.userInteractionService = userInteractionService;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public string Scope
        {
            get { return this.scope; }
            set
            {
                this.scope = value;
                this.RaisePropertyChanged();
            }
        }

        public DateTime WhenGenerated
        {
            get { return this.whenGenerated; }
            set
            {
                this.whenGenerated = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsPackageBuild
        {
            get { return this.isPackageBuild; }
            set { this.RaiseAndSetIfChanged(ref this.isPackageBuild, value); }
        }

        public bool IsPackageSendingAttemptCompleted
        {
            get { return this.isPackageSendingAttemptCompleted; }
            set { this.RaiseAndSetIfChanged(ref this.isPackageSendingAttemptCompleted, value); }
        }

        public string PackageSendingAttemptResponceText
        {
            get { return this.packageSendingAttemptResponceText; }
            set { this.RaiseAndSetIfChanged(ref this.packageSendingAttemptResponceText, value); }
        }

        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.RaiseAndSetIfChanged(ref this.isInProgress, value); }
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

        private async Task SendBackupAsync()
        {
            if (this.IsInProgress)
                return;

            this.IsInProgress = true;
            var cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await this.synchronizationService.SendBackupAsync(informationPackageFilePath, cancellationTokenSource.Token);
                this.PackageSendingAttemptResponceText = InterviewerUIResources.Troubleshooting_InformationPackageIsSuccessfullySent;
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