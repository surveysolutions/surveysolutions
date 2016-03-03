using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class SendTabletInformationViewModel : BaseViewModel
    {
        private readonly IBackupRestoreService backupRestoreService;
        private readonly ISynchronizationService synchronizationService;
        private readonly IUserInteractionService userInteractionService;
        private readonly ILogger logger;

        private string scope;
        private DateTime whenGenerated;
        private bool isPackageBuild;
        private bool isPackageSendingAttemptCompleted;
        private string packageSendingAttemptResponceText;
        private bool isInProgress;
        private byte[] informationPackageContent;

        public SendTabletInformationViewModel(
            IBackupRestoreService backupRestoreService,
            ISynchronizationService synchronizationService,
            ILogger logger, 
            IUserInteractionService userInteractionService)
        {
            this.backupRestoreService = backupRestoreService;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.userInteractionService = userInteractionService;
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

        public byte[] InformationPackageContent
        {
            get { return this.informationPackageContent; }
            set { this.RaiseAndSetIfChanged(ref this.informationPackageContent, value); }
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

        private async Task CreateTabletInformation()
        {
            if (this.IsInProgress)
                return;

            this.IsPackageBuild = false;
            this.IsPackageSendingAttemptCompleted = false;
            this.IsInProgress = true;

            try
            {
                var backupStream = await this.backupRestoreService.GetSystemBackupAsync();
                this.IsPackageBuild = true;
                this.Scope = FileSizeUtils.SizeSuffix(backupStream.Length);
                this.WhenGenerated = DateTime.Now;
                this.InformationPackageContent = backupStream;
            }
            catch (Exception e)
            {
                this.logger.Error(e.Message, e);
                await this.userInteractionService.AlertAsync(e.Message);
            }

            this.IsInProgress = false;
        }

        private async Task SendTabletInformation()
        {
            if (this.IsInProgress)
                return;

            this.IsInProgress = true;
            var cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await this.synchronizationService.SendTabletInformationAsync(
                    Convert.ToBase64String(this.InformationPackageContent), cancellationTokenSource.Token);
                this.PackageSendingAttemptResponceText =
                    InterviewerUIResources.Troubleshooting_InformationPackageIsSuccessfullySent;
            }
            catch (SynchronizationException ex)
            {
                this.logger.Error("Error when sending tablet info. ", ex);
                this.PackageSendingAttemptResponceText = ex.Message;
            }
            this.IsPackageSendingAttemptCompleted = true;
            this.IsPackageBuild = false;
            this.IsInProgress = false;
        }

        private void DeleteTabletInformation()
        {
            this.IsPackageBuild = false;
        }

        public IMvxCommand CreateTabletInformationCommand
        {
            get { return new MvxCommand(async () => await this.CreateTabletInformation()); }
        }

        public IMvxCommand SendTabletInformationCommand
        {
            get { return new MvxCommand(async () => await this.SendTabletInformation()); }
        }

        public IMvxCommand DeleteTabletInformationCommand
        {
            get { return new MvxCommand(DeleteTabletInformation); }
        }
    }
}