using System;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class BackupRestoreViewModel : BaseViewModel
    {
        private readonly ITroubleshootingService troubleshootingService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private bool isRestoreVisible;
        private bool isBackupInProgress;
        private bool isBackupCreated;
        private int clicksCountOnDescriptionPanel;
        private string backupLocation;
        private DateTime backupCreationDate;
        private string backupScope;

        public BackupRestoreViewModel(
            ITroubleshootingService troubleshootingService, 
            IUserInteractionService userInteractionService, IInterviewerSettings interviewerSettings, IFileSystemAccessor fileSystemAccessor)
        {
            this.troubleshootingService = troubleshootingService;
            this.userInteractionService = userInteractionService;
            this.interviewerSettings = interviewerSettings;
            this.fileSystemAccessor = fileSystemAccessor;
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

        public IMvxCommand BackupCommand
        {
            get { return new MvxCommand(async () => await BackupAsync()); }
        }

        public IMvxCommand RestoreCommand
        {
            get { return new MvxCommand(async () => await RestoreAsync()); }
        }

        public IMvxCommand IncreaseClicksCountOnDescriptionPanelCommand
        {
            get
            {
                return new MvxCommand(() =>
                {
                    clicksCountOnDescriptionPanel++;
                    if (this.clicksCountOnDescriptionPanel > 10)
                    {
                        this.IsRestoreVisible = true;
                        this.IsBackupCreated = false;
                        this.clicksCountOnDescriptionPanel = 0;
                    }
                });
            }
        }

        private async Task BackupAsync()
        {
            this.IsBackupCreated = false;
            this.IsBackupInProgress = true;
            var pathToFolder = this.fileSystemAccessor.CombinePath(this.interviewerSettings.GetExternalStorageDirectory(), "Backup");
            var createdFileName = await this.troubleshootingService.BackupAsync(pathToFolder);
            this.IsBackupInProgress = false;
            this.IsBackupCreated = true;
            this.BackupLocation = createdFileName;
            this.BackupCreationDate=DateTime.Now;
            this.BackupScope= FileSizeUtils.SizeSuffix(this.fileSystemAccessor.GetFileSize(createdFileName));
        }

        private async Task RestoreAsync()
        {
             var pathToFolder = this.fileSystemAccessor.CombinePath(this.interviewerSettings.GetExternalStorageDirectory(), "Restore");
            var filesInRestoreFolder = this.fileSystemAccessor.GetFilesInDirectory(pathToFolder);
            if (!filesInRestoreFolder.Any())
                return;
            if (await this.userInteractionService.ConfirmAsync(
                InterviewerUIResources.Troubleshooting_RestoreConfirmation.FormatString(filesInRestoreFolder[0]),
                string.Empty, UIResources.Yes, UIResources.No))
            {
                this.IsBackupInProgress = true;

                await Task.Run(() => this.troubleshootingService.Restore(filesInRestoreFolder[0]));
                this.IsBackupInProgress = false;

                await userInteractionService.AlertAsync(InterviewerUIResources.Troubleshooting_RestoredSuccessfully);
            }
        }
    }
}