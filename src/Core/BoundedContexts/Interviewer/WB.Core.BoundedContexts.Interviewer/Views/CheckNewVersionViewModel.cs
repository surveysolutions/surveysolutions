using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class CheckNewVersionViewModel : MvxNotifyPropertyChanged
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly ITabletDiagnosticService tabletDiagnosticService;
        private readonly ILogger logger;
        private CancellationTokenSource cancellationTokenSource;

        private bool isVersionCheckInProgress;
        private bool isNewVersionAvaliable;
        private string checkNewVersionResult;

        public CheckNewVersionViewModel(ISynchronizationService synchronizationService, 
            IInterviewerSettings interviewerSettings, 
            ITabletDiagnosticService tabletDiagnosticService, 
            ILogger logger)
        {
            this.synchronizationService = synchronizationService;
            this.interviewerSettings = interviewerSettings;
            this.tabletDiagnosticService = tabletDiagnosticService;
            this.logger = logger;
            Version = this.interviewerSettings.GetApplicationVersionName();
            this.cancellationTokenSource = new CancellationTokenSource(this.downloadApkTimeout);
        }

        public bool IsVersionCheckInProgress
        {
            get { return this.isVersionCheckInProgress; }
            set { this.RaiseAndSetIfChanged(ref this.isVersionCheckInProgress, value); }
        }

        public bool IsNewVersionAvaliable
        {
            get { return this.isNewVersionAvaliable; }
            set { this.RaiseAndSetIfChanged(ref this.isNewVersionAvaliable, value); }
        }

        public string CheckNewVersionResult
        {
            get { return this.checkNewVersionResult; }
            set { this.RaiseAndSetIfChanged(ref this.checkNewVersionResult, value); }
        }

        public string Version { get; set; }

        public IMvxCommand CheckVersionCommand
        {
            get { return new MvxCommand(async () => await this.CheckVersion()); }
        }

        public IMvxCommand CancelUpgrade
        {
            get { return new MvxCommand(() => this.cancellationTokenSource.Cancel());}
        }

        private MvxAsyncCommand updateApplicationCommand;
        private TimeSpan downloadApkTimeout = TimeSpan.FromMinutes(30);

        public IMvxCommand UpdateApplicationCommand
        {
            get
            {
                return this.updateApplicationCommand ?? (this.updateApplicationCommand = new MvxAsyncCommand(async () => await this.UpdateApplication()));
            }
        }

        private async Task UpdateApplication()
        {
            this.IsNewVersionAvaliable = false;
            this.IsVersionCheckInProgress = true;
            this.cancellationTokenSource = new CancellationTokenSource(this.downloadApkTimeout);
            try
            {
                this.CheckNewVersionResult = InterviewerUIResources.Diagnostics_DownloadingPleaseWait;

                await
                    this.tabletDiagnosticService.UpdateTheApp(this.interviewerSettings.Endpoint,
                        this.cancellationTokenSource.Token,
                        this.downloadApkTimeout);

                this.CheckNewVersionResult = null;
            }
            catch (Exception ex)
            {
                this.logger.Error("Error when updating", ex);
                this.CheckNewVersionResult = ex.Message;
            }
            this.IsVersionCheckInProgress = false;
        }

        private async Task CheckVersion()
        {
            this.IsNewVersionAvaliable = false;
            this.CheckNewVersionResult = null;
            this.IsVersionCheckInProgress = true;
            var cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var versionFromServer = await
                    this.synchronizationService.GetLatestApplicationVersionAsync(cancellationTokenSource.Token);

                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    if (versionFromServer.HasValue && versionFromServer > this.interviewerSettings.GetApplicationVersionCode())
                    {
                        this.IsNewVersionAvaliable = true;
                    }
                    else
                    {
                        this.CheckNewVersionResult =
                            InterviewerUIResources.Diagnostics_YouHaveTheLatestVersionOfApplication;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error("Error when checking for update", ex);
                this.CheckNewVersionResult = ex.Message;
            }

            this.IsVersionCheckInProgress = false;
        }

        private void RejectUpdateApplication()
        {
            this.IsNewVersionAvaliable = false;
        }

        public IMvxCommand RejectUpdateApplicationCommand
        {
            get { return new MvxCommand(RejectUpdateApplication); }
        }
    }
}