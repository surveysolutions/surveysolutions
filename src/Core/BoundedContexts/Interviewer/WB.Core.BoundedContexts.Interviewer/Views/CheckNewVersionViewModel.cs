using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class CheckNewVersionViewModel : BaseViewModel
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly ITabletDiagnosticService tabletDiagnosticService;
        private readonly ILogger logger;

        private bool isVersionCheckInProgress;
        private bool isNewVersionAvaliable;
        private int latestApplicationVersion;
        private string checkNewVersionResult;

        public CheckNewVersionViewModel(ISynchronizationService synchronizationService, IUserInteractionService userInteractionService, IInterviewerSettings interviewerSettings, ITabletDiagnosticService tabletDiagnosticService, ILogger logger)
        {
            this.synchronizationService = synchronizationService;
            this.userInteractionService = userInteractionService;
            this.interviewerSettings = interviewerSettings;
            this.tabletDiagnosticService = tabletDiagnosticService;
            this.logger = logger;
            Version = this.interviewerSettings.GetApplicationVersionName();
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

        public int LatestApplicationVersion
        {
            get { return this.latestApplicationVersion; }
            set { this.RaiseAndSetIfChanged(ref this.latestApplicationVersion, value); }
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

        public IMvxCommand UpdateApplicationCommand
        {
            get { return new MvxCommand(async () => await this.UpdateApplication()); }
        }

        private async Task UpdateApplication()
        {
            this.IsNewVersionAvaliable = false;
            this.IsVersionCheckInProgress = true;
            try
            {
                this.CheckNewVersionResult =
                        InterviewerUIResources.Diagnostics_DownloadingPleaseWait;
                await Task.Run(() => this.tabletDiagnosticService.UpdateTheApp(this.interviewerSettings.Endpoint));

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
                        this.LatestApplicationVersion = versionFromServer.Value;
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