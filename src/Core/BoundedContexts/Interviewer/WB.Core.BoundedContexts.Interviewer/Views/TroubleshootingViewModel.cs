using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class TroubleshootingViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly ITroubleshootingService troubleshootingService;
        private readonly IUserInteractionService userInteractionService;
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;
        private readonly IViewModelNavigationService viewModelNavigationService;

        public TroubleshootingViewModel(
            IViewModelNavigationService viewModelNavigationService, 
            IPrincipal principal,
            ITroubleshootingService troubleshootingService,
            IUserInteractionService userInteractionService,
            ISynchronizationService synchronizationService,
            ILogger logger)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.troubleshootingService = troubleshootingService;
            this.userInteractionService = userInteractionService;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
        }

        private CancellationTokenSource cancellationTokenSource;

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.RaiseAndSetIfChanged(ref this.isInProgress, value); }
        }

        public IMvxCommand NavigateToLoginCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToAsync<LoginViewModel>()); }
        }
        
        public IMvxCommand NavigateToDashboardCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToDashboardAsync()); }
        }

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return this.signOutCommand ?? (this.signOutCommand = new MvxCommand(async () => await this.SignOut())); }
        }

        private IMvxCommand sendTabletInformationCommand;
        public IMvxCommand SendTabletInformationCommand
        {
            get
            {
                return this.sendTabletInformationCommand ?? (this.sendTabletInformationCommand =
                    new MvxCommand(async () => await this.SendTabletInformationAsync(), () => !this.IsInProgress));
            }
        }

        private IMvxCommand backupCommand;
        public IMvxCommand BackupCommand
        {
            get
            {
                return this.backupCommand ?? (this.backupCommand =
                    new MvxCommand(async () => await this.BackupAsync(), () => !this.IsInProgress));
            }
        }

        private IMvxCommand restoreCommand;
        public IMvxCommand RestoreCommand
        {
            get
            {
                return this.restoreCommand ?? (this.restoreCommand =
                    new MvxCommand(async () => await this.RestoreAsync(), () => !this.IsInProgress));
            }
        }

        private IMvxCommand cancelSendingTabletInformationCommand;
        public IMvxCommand CancelSendingTabletInformationCommand
            => this.cancelSendingTabletInformationCommand ?? (this.cancelSendingTabletInformationCommand = new MvxCommand(this.CancelSendingTabletInformation));


        private async Task RestoreAsync()
        {
            await this.troubleshootingService.RestoreAsync("");
        }

        private async Task BackupAsync()
        {
            await this.troubleshootingService.BackupAsync("");
        }
        
        private void CancelSendingTabletInformation()
        {
            if (!this.cancellationTokenSource.IsCancellationRequested)
                this.cancellationTokenSource.Cancel();
        }
        
        private async Task SendTabletInformationAsync()
        {
            this.IsInProgress = true;
            this.cancellationTokenSource = new CancellationTokenSource();

            var backupStream = await this.troubleshootingService.GetSystemBackupAsync();

            if (!this.cancellationTokenSource.IsCancellationRequested)
            {
                if (await this.userInteractionService.ConfirmAsync(
                   string.Format(InterviewerUIResources.Troubleshooting_Old_InformationPackageSizeWarningFormat,
                       FileSizeUtils.SizeSuffix(backupStream.Length)), string.Empty, UIResources.Yes, UIResources.Cancel))
                {
                    try
                    {
                        await this.synchronizationService.SendTabletInformationAsync(
                                Convert.ToBase64String(backupStream), this.cancellationTokenSource.Token);
                        await userInteractionService.AlertAsync(InterviewerUIResources.Troubleshooting_InformationPackageIsSuccessfullySent);
                    }
                    catch (SynchronizationException ex)
                    {
                        this.logger.Error("Error when sending tablet info. ", ex);
                        await userInteractionService.AlertAsync(ex.Message, InterviewerUIResources.Warning);
                    }
                }
            }

            this.IsInProgress = false;
        }

        private async Task SignOut()
        {
            this.principal.SignOut();
            await this.viewModelNavigationService.NavigateToAsync<LoginViewModel>();
        }

        public bool IsAuthenticated => this.principal.IsAuthenticated;
    }
}
