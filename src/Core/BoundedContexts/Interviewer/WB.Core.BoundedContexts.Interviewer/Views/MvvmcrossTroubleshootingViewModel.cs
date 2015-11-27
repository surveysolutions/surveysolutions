using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.ErrorReporting.Services.CapiInformationService;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class MvvmcrossTroubleshootingViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly INetworkService networkService;
        private readonly IUserInteractionService userInteractionService;
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;
        private readonly IInterviewerApplicationUpdater updater;

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICapiInformationService capiInformationService;

        public MvvmcrossTroubleshootingViewModel(
            IViewModelNavigationService viewModelNavigationService, 
            IInterviewerSettings interviewerSettings,
            INetworkService networkService, 
            IUserInteractionService userInteractionService, 
            ISynchronizationService synchronizationService, 
            ILogger logger, 
            IInterviewerApplicationUpdater updater, 
            IFileSystemAccessor fileSystemAccessor, 
            ICapiInformationService capiInformationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewerSettings = interviewerSettings;
            this.networkService = networkService;
            this.userInteractionService = userInteractionService;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.updater = updater;
            this.fileSystemAccessor = fileSystemAccessor;
            this.capiInformationService = capiInformationService;
        }

        public void Init()
        {
            Version = this.interviewerSettings.GetApplicationVersionName();
            IsRestoreVisible = false;
        }

        public bool IsRestoreVisible
        {
            get { return this.isRestoreVisible; }
            set { this.isRestoreVisible = value; this.RaisePropertyChanged(); }
        }

        public string Version { get; set; }

        private int clickCount = 0;
        const int NUMBER_CLICK = 10;

        private IMvxCommand countClicksCommand;
        public IMvxCommand CountClicksCommand
        {
            get { return this.countClicksCommand ?? (this.countClicksCommand = new MvxCommand(this.CountClicks, () => !IsInProgress)); }
        }

        private void CountClicks()
        {
            clickCount++;
            if (this.clickCount != NUMBER_CLICK)
            {
                return;
            }

            this.clickCount = 0;
            this.IsRestoreVisible = true;
        }

        private IMvxCommand checkNewVersionCommand;
        public IMvxCommand CheckNewVersionCommand
        {
            get { return this.checkNewVersionCommand ?? (this.checkNewVersionCommand = new MvxCommand(async () => await this.CheckNewVersion(), () => !IsInProgress)); }
        }

        //private IMvxCommand backupCommand;
        //public IMvxCommand BackupCommand
        //{
        //    get { return this.backupCommand ?? (this.backupCommand = new MvxCommand(async () => await this.Backup(), () => !IsInProgress)); }
        //}

        //private IMvxCommand restoreCommand;
        //public IMvxCommand RestoreCommand
        //{
        //    get { return this.restoreCommand ?? (this.restoreCommand = new MvxCommand(async () => await this.Restore(), () => !IsInProgress)); }
        //}

        private IMvxCommand sendTabletInformationCommand;
        public IMvxCommand SendTabletInformationCommand
        {
            get { return this.sendTabletInformationCommand ?? (this.sendTabletInformationCommand = new MvxCommand(async () => await this.SendTabletInformation(), () => !IsInProgress)); }
        }

        //private async Task Backup()
        //{
        //    IsInProgress = true;

        //    string path = string.Empty;
        //    try
        //    {
        //        path = this.backupManager.Backup();
        //    }
        //    catch (Exception exception)
        //    {
        //        logger.Error("Error occured during Backup. ", exception);
        //    }
           
        //    if (string.IsNullOrWhiteSpace(path))
        //    {
        //        await userInteractionService.AlertAsync(InterviewerUIResources.Troubleshooting_BackupErrorMessage);
        //    }
        //    else
        //    {
        //        await userInteractionService.AlertAsync(string.Format(InterviewerUIResources.Troubleshooting_BackupWasSaved, path));
        //    }

        //    IsInProgress = false;
        //}

        //private async Task Restore()
        //{
        //    var shouldWeProceedRestore = await userInteractionService.ConfirmAsync(
        //        string.Format(InterviewerUIResources.Troubleshooting_RestoreConfirmation, this.backupManager.RestorePath));

        //    if (!shouldWeProceedRestore) return;
        //    var wasErrorHappened = false;
        //    IsInProgress = true;
        //    try
        //    {
        //        this.backupManager.Restore();

        //        await userInteractionService.AlertAsync(InterviewerUIResources.Troubleshooting_RestoredSuccessfully);
        //    }
        //    catch (Exception exception)
        //    {
        //        logger.Error("Error occured during Restore. ", exception);
        //        wasErrorHappened = true;
                
        //    }
        //    if (wasErrorHappened)
        //    {
        //        await userInteractionService.AlertAsync(InterviewerUIResources.Troubleshooting_RestorationErrorMessage);
        //    }

        //    IsInProgress = false;
        //}

        private async Task CheckNewVersion()
        {
            if (!this.networkService.IsNetworkEnabled())
            {
                await userInteractionService.AlertAsync(InterviewerUIResources.NoNetwork);
                return;
            }

            IsInProgress = true;
            string errorMessageHappened = null;
            bool isNewVersionAvailable = false;
            try
            {
                isNewVersionAvailable = (await this.synchronizationService.GetLatestApplicationVersionAsync(token: default(CancellationToken))).Value 
                    > this.interviewerSettings.GetApplicationVersionCode();
            }
            catch (SynchronizationException ex)
            {
                this.logger.Error("Check new version. SynchronizationException. ", ex);
                errorMessageHappened = ex.Message;
            }
            catch (Exception ex)
            {
                this.logger.Error("Check new version. Unexpected exception", ex);
                errorMessageHappened = ex.Message;
            }

            if (!string.IsNullOrEmpty(errorMessageHappened))
            {
                await userInteractionService.AlertAsync(errorMessageHappened);
                IsInProgress = false;
                return;
            }

            if (isNewVersionAvailable)
            {
                var shouldWeDownloadUpgrade = await userInteractionService.ConfirmAsync(InterviewerUIResources.Troubleshooting_NewVerisonExist);

                if (shouldWeDownloadUpgrade)
                {
                    try
                    {
                        updater.GetLatestVersion();
                    }
                    catch (Exception exception)
                    {
                        this.logger.Error("Check new version. Unexpected exception when downloading app", exception);
                        errorMessageHappened = InterviewerUIResources.Troubleshooting_Unknown_ErrorMessage;
                    }
                }
            }
            else
            {
                await userInteractionService.AlertAsync(InterviewerUIResources.Troubleshooting_NoNewVersion);
            }

            if (!string.IsNullOrEmpty(errorMessageHappened))
            {
                await userInteractionService.AlertAsync(errorMessageHappened);
            }
            IsInProgress = false;
        }

        private async Task SendTabletInformation()
        {
            IsInProgress = true;

            var tokenSource = new CancellationTokenSource();

            string pathToInfoArchive = string.Empty;
            string errorMessageHappened = null;
            try
            {
                pathToInfoArchive = await this.capiInformationService.CreateInformationPackage(tokenSource.Token);

                if (string.IsNullOrEmpty(pathToInfoArchive) || !this.fileSystemAccessor.IsFileExists(pathToInfoArchive))
                {
                    await userInteractionService.AlertAsync("No file");
                    IsInProgress = false;
                    return;
                }

                var formattedFileSize = FileSizeUtils.SizeSuffix(this.fileSystemAccessor.GetFileSize(pathToInfoArchive));
                var shouldWeSendTabletInformation = await userInteractionService.ConfirmAsync(string.Format(InterviewerUIResources.Troubleshooting_InformationPackageSizeWarningFormat, formattedFileSize));

                if (shouldWeSendTabletInformation)
                {
                    await this.synchronizationService.SendTabletInformationAsync(Convert.ToBase64String(this.fileSystemAccessor.ReadAllBytes(pathToInfoArchive)), tokenSource.Token);
                    await userInteractionService.AlertAsync(InterviewerUIResources.Troubleshooting_InformationPackageIsSuccessfullySent);
                }
            }
            catch (Exception exception)
            {
                logger.Error("Error occured during Restore. ", exception);
                errorMessageHappened = InterviewerUIResources.Troubleshooting_SendingOfInformationPackageErrorMessage;
            }
            finally
            {
                if (this.fileSystemAccessor.IsFileExists(pathToInfoArchive))
                    this.fileSystemAccessor.DeleteFile(pathToInfoArchive);
            }
            if (string.IsNullOrEmpty(errorMessageHappened))
            {
                await userInteractionService.AlertAsync(errorMessageHappened);
            }
            IsInProgress = false;
        }

        private bool isInProgress;
        private bool isRestoreVisible;

        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; RaisePropertyChanged(); }
        }

        public IMvxCommand NavigateToTroubleshootingPageCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToAsync<TroubleshootingViewModel>()); }
        }

        public IMvxCommand NavigateToDashboardCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToDashboardAsync()); }
        }
    }
}