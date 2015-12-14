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

        public string Version { get; set; }

        public IMvxCommand CheckVersionCommand
        {
            get { return new MvxCommand(async () => await this.CheckVersion()); }
        }

        private async Task CheckVersion()
        {
            if (this.IsVersionCheckInProgress)
                return;
            this.IsVersionCheckInProgress = true;
            try
            {
                var newVersionAvailableOrNullIfThrow =
                    (await
                        this.synchronizationService.GetLatestApplicationVersionAsync(token: default(CancellationToken)))
                        .Value > this.interviewerSettings.GetApplicationVersionCode();
                if (newVersionAvailableOrNullIfThrow)
                    if (
                        await
                            this.userInteractionService.ConfirmAsync(
                                "New version exists. Would you like to download and update application?", string.Empty,
                                UIResources.Yes,
                                UIResources.Cancel))
                    {
                        await Task.Run(() => this.tabletDiagnosticService.UpdateTheApp(this.interviewerSettings.Endpoint));
                    }
            }
            catch (Exception ex)
            {
                this.logger.Error("Error when sending tablet info. ", ex);
                await userInteractionService.AlertAsync(ex.Message, InterviewerUIResources.Warning);
            }
            this.IsVersionCheckInProgress = false;
        }
    }
}