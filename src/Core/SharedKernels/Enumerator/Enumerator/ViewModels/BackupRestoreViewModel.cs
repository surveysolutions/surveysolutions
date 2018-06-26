using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class BackupRestoreViewModel : MvxNotifyPropertyChanged
    {
        private readonly IBackupRestoreService backupRestoreService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IDeviceSettings deviceSettings;
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
            IDeviceSettings deviceSettings, 
            IFileSystemAccessor fileSystemAccessor, 
            ITabletDiagnosticService tabletDiagnosticService, 
            ILogger logger)
        {
            this.backupRestoreService = backupRestoreService;
            this.userInteractionService = userInteractionService;
            this.deviceSettings = deviceSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabletDiagnosticService = tabletDiagnosticService;
            this.logger = logger;
        }

        public bool IsRestoreVisible
        {
            get => this.isRestoreVisible;
            set { this.isRestoreVisible = value; this.RaisePropertyChanged(); }
        }

        public bool IsBackupCreated
        {
            get => this.isBackupCreated;
            set { this.isBackupCreated = value; this.RaisePropertyChanged(); }
        }

        public bool IsBackupInProgress
        {
            get => this.isBackupInProgress;
            set { this.isBackupInProgress = value; this.RaisePropertyChanged(); }
        }

        public string BackupLocation
        {
            get => this.backupLocation;
            set { this.backupLocation = value; this.RaisePropertyChanged(); }
        }

        public DateTime BackupCreationDate
        {
            get => this.backupCreationDate;
            set { this.backupCreationDate = value; this.RaisePropertyChanged(); }
        }

        public string BackupScope
        {
            get => this.backupScope;
            set { this.backupScope = value; this.RaisePropertyChanged(); }
        }

        public string RestoreLocation
        {
            get => this.restoreLocation;
            set { this.restoreLocation = value; this.RaisePropertyChanged(); }
        }

        public DateTime RestoreCreationDate
        {
            get => this.restoreCreationDate;
            set { this.restoreCreationDate = value; this.RaisePropertyChanged(); }
        }

        public string RestoreScope
        {
            get => this.restoreScope;
            set { this.restoreScope = value; this.RaisePropertyChanged(); }
        }

        public IMvxAsyncCommand BackupCommand => new MvxAsyncCommand(this.BackupAsync);
        public IMvxCommand RestoreCommand => new MvxAsyncCommand(this.RestoreAsync);
        public IMvxCommand IncreaseClicksCountOnDescriptionPanelCommand => new MvxAsyncCommand(this.IncreaseClicksCountOnDescriptionPanel);

        private async Task IncreaseClicksCountOnDescriptionPanel()
        {
            if (this.IsBackupInProgress)
                return;

            this.clicksCountOnDescriptionPanel++;
            if (this.clicksCountOnDescriptionPanel > 10)
            {
                this.clicksCountOnDescriptionPanel = 0;

                var restoreFolder = this.deviceSettings.RestoreFolder;

                try
                {
                    var package = await this.backupRestoreService.GetRestorePackageInfo(restoreFolder);
                    if (package != null)
                    {
                        this.IsRestoreVisible = true;
                        this.IsBackupCreated = false;
                        this.RestoreLocation = package.FileLocation;
                        this.RestoreScope = FileSizeUtils.SizeSuffix(package.FileSize);
                        this.RestoreCreationDate = package.FileCreationDate;
                    }
                    else 
                    {
                        await this.userInteractionService.AlertAsync(InterviewerUIResources.Troubleshooting_RestoreFolderIsEmpty.FormatString(restoreFolder));
                    }
                }
                catch (MissingPermissionsException e)
                {
                    await this.userInteractionService.AlertAsync(e.Message);
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
                    this.backupRestoreService.BackupAsync(this.deviceSettings.BackupFolder)
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
