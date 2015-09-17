using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Backup;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class TroubleshootingViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly INetworkService networkService;
        private readonly IUserInteractionService userInteractionService;
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;
        private readonly IInterviewerApplicationUpdater updater;
        private readonly IBackup backupManager;

        public TroubleshootingViewModel(
            IViewModelNavigationService viewModelNavigationService, 
            IInterviewerSettings interviewerSettings,
            INetworkService networkService, 
            IUserInteractionService userInteractionService, 
            ISynchronizationService synchronizationService, 
            ILogger logger, IInterviewerApplicationUpdater updater, IBackup backupManager)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewerSettings = interviewerSettings;
            this.networkService = networkService;
            this.userInteractionService = userInteractionService;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.updater = updater;
            this.backupManager = backupManager;
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

        private IMvxCommand backupCommand;
        public IMvxCommand BackupCommand
        {
            get { return this.backupCommand ?? (this.backupCommand = new MvxCommand(async () => await this.Backup(), () => !IsInProgress)); }
        }

        private IMvxCommand restoreCommand;
        public IMvxCommand RestoreCommand
        {
            get { return this.restoreCommand ?? (this.restoreCommand = new MvxCommand(async () => await this.Restore(), () => !IsInProgress)); }
        }

        private async Task Backup()
        {
            IsInProgress = true;

            string path = string.Empty;
            try
            {
                path = this.backupManager.Backup();
            }
            catch (Exception exception)
            {
                logger.Error("Error occured during Backup. ", exception);
            }
           
            if (string.IsNullOrWhiteSpace(path))
            {
                await userInteractionService.AlertAsync(UIResources.Troubleshooting_BackupErrorMessage);
            }
            else
            {
                await userInteractionService.AlertAsync(string.Format(UIResources.Troubleshooting_BackupWasSaved, path));
            }

            IsInProgress = false;
        }

        private async Task Restore()
        {
            var shouldWeProceedRestore = await userInteractionService.ConfirmAsync(
                string.Format(UIResources.Troubleshooting_RestoreConfirmation, this.backupManager.RestorePath));

            if (!shouldWeProceedRestore) return;

            IsInProgress = true;
            try
            {
                this.backupManager.Restore();

                await userInteractionService.AlertAsync(UIResources.Troubleshooting_RestoredSuccessfully);
            }
            catch (Exception exception)
            {
                logger.Error("Error occured during Restore. ", exception);
                await userInteractionService.AlertAsync(UIResources.Troubleshooting_RestorationErrorMessage);
            }
            IsInProgress = false;
        }

        private async Task CheckNewVersion()
        {
            if (!this.networkService.IsNetworkEnabled())
            {
                await userInteractionService.AlertAsync(UIResources.NoNetwork);
                return;
            }

            IsInProgress = true;

            bool isNewVersionAvailable = false;
            try
            {
                isNewVersionAvailable = (await this.synchronizationService.GetLatestApplicationVersionAsync(token: default(CancellationToken))).Value 
                    > this.interviewerSettings.GetApplicationVersionCode();
            }
            catch (SynchronizationException ex)
            {
                await userInteractionService.AlertAsync(ex.Message);
                IsInProgress = false;
                return;
            }
            catch (Exception ex)
            {
                this.logger.Error("Check new version. Unexpected exception", ex);
                IsInProgress = false;
                return;
            }

            if (isNewVersionAvailable)
            {
                var shouldWeDownloadUpgrade = await userInteractionService.ConfirmAsync(UIResources.Troubleshooting_NewVerisonExist);

                if (shouldWeDownloadUpgrade)
                {
                    try
                    {
                        updater.GetLatestVersion();
                    }
                    catch (Exception exception)
                    {
                        this.logger.Error("Check new version. Unexpected exception when downloading app", exception);
                    }
                }
            }
            else
            {
                await userInteractionService.AlertAsync(UIResources.Troubleshooting_NoNewVersion);
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
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateTo<TroubleshootingViewModel>()); }
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }

        public IMvxCommand NavigateToDashboardCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateToDashboard()); }
        }
    }
}