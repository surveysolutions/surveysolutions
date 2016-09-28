using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class BackupRestoreViewModel : MvxNotifyPropertyChanged
    {
        private readonly IBackupRestoreService backupRestoreService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ITabletDiagnosticService tabletDiagnosticService;
        private readonly ILogger logger;

        private bool isRestoreVisible;
        private bool isBackupInProgress;
        private bool isBackupCreated;
        private int clicksCountOnDescriptionPanel;
        private string backupLocation;
        private DateTime backupCreationDate;
        private string backupScope;
        private string restoreLocation;
        private DateTime restoreCreationDate;
        private string restoreScope;

        public BackupRestoreViewModel(
            IBackupRestoreService backupRestoreService, 
            IUserInteractionService userInteractionService, 
            IInterviewerSettings interviewerSettings, 
            IFileSystemAccessor fileSystemAccessor, 
            ITabletDiagnosticService tabletDiagnosticService, 
            ILogger logger)
        {
            this.backupRestoreService = backupRestoreService;
            this.userInteractionService = userInteractionService;
            this.interviewerSettings = interviewerSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabletDiagnosticService = tabletDiagnosticService;
            this.logger = logger;
        }

        public bool IsRestoreVisible
        {
            get { return this.isRestoreVisible; }
            set { this.isRestoreVisible = value; this.RaisePropertyChanged(); }
        }

        public bool IsBackupCreated
        {
            get { return this.isBackupCreated; }
            set { this.isBackupCreated = value; this.RaisePropertyChanged(); }
        }

        public bool IsBackupInProgress
        {
            get { return this.isBackupInProgress; }
            set { this.isBackupInProgress = value; this.RaisePropertyChanged(); }
        }

        public string BackupLocation
        {
            get { return this.backupLocation; }
            set { this.backupLocation = value; this.RaisePropertyChanged(); }
        }

        public DateTime BackupCreationDate
        {
            get { return this.backupCreationDate; }
            set { this.backupCreationDate = value; this.RaisePropertyChanged(); }
        }

        public string BackupScope
        {
            get { return this.backupScope; }
            set { this.backupScope = value; this.RaisePropertyChanged(); }
        }

        public string RestoreLocation
        {
            get { return this.restoreLocation; }
            set { this.restoreLocation = value; this.RaisePropertyChanged(); }
        }

        public DateTime RestoreCreationDate
        {
            get { return this.restoreCreationDate; }
            set { this.restoreCreationDate = value; this.RaisePropertyChanged(); }
        }

        public string RestoreScope
        {
            get { return this.restoreScope; }
            set { this.restoreScope = value; this.RaisePropertyChanged(); }
        }

        public IMvxAsyncCommand BackupCommand => new MvxAsyncCommand(this.BackupAsync);

        public IMvxCommand RestoreCommand
        {
            get { return new MvxCommand(async () => await RestoreAsync()); }
        }

        public IMvxCommand IncreaseClicksCountOnDescriptionPanelCommand => new MvxCommand(async () => await this.IncreaseClicksCountOnDescriptionPanel());

        private async Task IncreaseClicksCountOnDescriptionPanel()
        {
            if (this.IsBackupInProgress)
                return;

            this.clicksCountOnDescriptionPanel++;
            if (this.clicksCountOnDescriptionPanel > 10)
            {
                this.clicksCountOnDescriptionPanel = 0;

                var restoreFolder = this.interviewerSettings.RestoreFolder;

                if (!this.fileSystemAccessor.IsDirectoryExists(restoreFolder))
                    this.fileSystemAccessor.CreateDirectory(restoreFolder);

                var filesInRestoreFolder = this.fileSystemAccessor.GetFilesInDirectory(restoreFolder);
                if (filesInRestoreFolder.Any())
                {
                    this.IsRestoreVisible = true;
                    this.IsBackupCreated = false;
                    this.RestoreLocation = filesInRestoreFolder[0];
                    this.RestoreScope = FileSizeUtils.SizeSuffix(this.fileSystemAccessor.GetFileSize(this.RestoreLocation));
                    this.RestoreCreationDate = this.fileSystemAccessor.GetCreationTime(this.RestoreLocation);
                }
                else
                {
                    await this.userInteractionService.AlertAsync(InterviewerUIResources.Troubleshooting_RestoreFolderIsEmpty.FormatString(restoreFolder));
                }
            }
        }

        private async Task BackupAsync()
        {
            if (this.IsBackupInProgress)
                return;

            this.IsBackupCreated = false;
            this.IsRestoreVisible = false;
            this.IsBackupInProgress = true;
            try
            {
                var createdFileName = await
                    this.backupRestoreService.BackupAsync(this.interviewerSettings.BackupFolder)
                        .ConfigureAwait(false);

                this.BackupLocation = createdFileName;
                this.BackupCreationDate = DateTime.Now;
                this.BackupScope = FileSizeUtils.SizeSuffix(this.fileSystemAccessor.GetFileSize(createdFileName));
                this.IsBackupCreated = true;
            }
            catch (Exception e)
            {
                this.logger.Error(e.Message, e);
                this.IsBackupCreated = false;
                await this.userInteractionService.AlertAsync(e.Message);
            }
            this.IsBackupInProgress = false;
        }

        private async Task RestoreAsync()
        {
            if (this.IsBackupInProgress)
                return;

            if (await this.userInteractionService.ConfirmAsync(
                InterviewerUIResources.Troubleshooting_RestoreConfirmation.FormatString(this.RestoreLocation),
                string.Empty, UIResources.Yes, UIResources.No))
            {
                this.IsBackupInProgress = true;
                try
                {
                    await this.backupRestoreService.RestoreAsync(this.RestoreLocation);
                    this.tabletDiagnosticService.RestartTheApp();
                }
                catch (Exception e)
                {
                    this.logger.Error(e.Message, e);
                    await this.userInteractionService.AlertAsync(e.Message);
                }

                this.IsBackupInProgress = false;
            }
        }
    }
}