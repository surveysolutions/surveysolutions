using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
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


        private bool visible;

        public bool IsTabletInformationPackageBuild
        {
            get { return this.visible; }
            set { this.RaiseAndSetIfChanged(ref this.visible, value); }
        }

        private bool isInProgress;

        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.RaiseAndSetIfChanged(ref this.isInProgress, value); }
        }

        private async Task SendTabletInformation()
        {
            // this.CanCancelProgress = true;
            this.IsInProgress = true;
            var cancellationTokenSource = new CancellationTokenSource();

            var backupStream = await Task.Run(() => this.troubleshootingService.GetSystemBackup());

            if (!cancellationTokenSource.IsCancellationRequested)
            {
                this.IsTabletInformationPackageBuild = true;
                this.Scope = FileSizeUtils.SizeSuffix(backupStream.Length);
                this.WhenGenerated = DateTime.Now;
                /*    if (await this.userInteractionService.ConfirmAsync(
                   string.Format(InterviewerUIResources.Troubleshooting_Old_InformationPackageSizeWarningFormat,
                       FileSizeUtils.SizeSuffix(backupStream.Length)), string.Empty, UIResources.Yes, UIResources.Cancel))
                {
                    try
                    {
                        await this.synchronizationService.SendTabletInformationAsync(
                                Convert.ToBase64String(backupStream), cancellationTokenSource.Token);
                        await userInteractionService.AlertAsync(InterviewerUIResources.Troubleshooting_InformationPackageIsSuccessfullySent);
                    }
                    catch (SynchronizationException ex)
                    {
                        this.logger.Error("Error when sending tablet info. ", ex);
                        await userInteractionService.AlertAsync(ex.Message, InterviewerUIResources.Warning);
                    }
                }*/
            }

            this.IsInProgress = false;
        }

        public IMvxCommand SendTabletInformationCommand
        {
            get { return new MvxCommand(async () => await this.SendTabletInformation()); }
        }

        public IMvxCommand DeleteTabletInformationCommand
        {
            get { return new MvxCommand(DeleteTabletInformation); }
        }

        private void DeleteTabletInformation()
        {
            this.IsTabletInformationPackageBuild = false;
        }
    }
}