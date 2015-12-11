using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class SendTabletInformationViewModel : BaseViewModel
    {
        private readonly ITroubleshootingService troubleshootingService;
        private readonly IUserInteractionService userInteractionService;
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;

        public SendTabletInformationViewModel(ITroubleshootingService troubleshootingService,
            IUserInteractionService userInteractionService, ISynchronizationService synchronizationService,
            ILogger logger)
        {
            this.troubleshootingService = troubleshootingService;
            this.userInteractionService = userInteractionService;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
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

        private string scope;

        public DateTime WhenGenerated
        {
            get { return this.whenGenerated; }
            set
            {
                this.whenGenerated = value;
                this.RaisePropertyChanged();
            }
        }

        private DateTime whenGenerated;

        private bool isPackageBuild;

        public bool IsPackageBuild
        {
            get { return this.isPackageBuild; }
            set { this.RaiseAndSetIfChanged(ref this.isPackageBuild, value); }
        }

        private bool isPackageSendingAttemptCompleted;

        public bool IsPackageSendingAttemptCompleted
        {
            get { return this.isPackageSendingAttemptCompleted; }
            set { this.RaiseAndSetIfChanged(ref this.isPackageSendingAttemptCompleted, value); }
        }

        private string packageSendingAttemptResponceText;

        public string PackageSendingAttemptResponceText
        {
            get { return this.packageSendingAttemptResponceText; }
            set { this.RaiseAndSetIfChanged(ref this.packageSendingAttemptResponceText, value); }
        }

        private bool isInProgress;

        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.RaiseAndSetIfChanged(ref this.isInProgress, value); }
        }

        private byte[] informationPackageContent = null;
        private async Task CreateTabletInformation()
        {
            this.IsPackageSendingAttemptCompleted = false;
            this.IsInProgress = true;
            var cancellationTokenSource = new CancellationTokenSource();

            var backupStream = await Task.Run(() => this.troubleshootingService.GetSystemBackup());

            if (!cancellationTokenSource.IsCancellationRequested)
            {
                this.IsPackageBuild = true;
                this.Scope = FileSizeUtils.SizeSuffix(backupStream.Length);
                this.WhenGenerated = DateTime.Now;
                this.informationPackageContent = backupStream;
            }

            this.IsInProgress = false;
        }

        private async Task SendTabletInformation()
        {
            this.IsInProgress = true;
            var cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await this.synchronizationService.SendTabletInformationAsync(
                    Convert.ToBase64String(this.informationPackageContent), cancellationTokenSource.Token);
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