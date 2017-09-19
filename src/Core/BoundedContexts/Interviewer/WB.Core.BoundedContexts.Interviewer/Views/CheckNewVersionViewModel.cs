﻿using System;
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
            get => this.isVersionCheckInProgress;
            set => this.RaiseAndSetIfChanged(ref this.isVersionCheckInProgress, value);
        }

        public bool IsNewVersionAvaliable
        {
            get => this.isNewVersionAvaliable;
            set => this.RaiseAndSetIfChanged(ref this.isNewVersionAvaliable, value);
        }

        public string CheckNewVersionResult
        {
            get => this.checkNewVersionResult;
            set => this.RaiseAndSetIfChanged(ref this.checkNewVersionResult, value);
        }

        public string Version { get; set; }

        public IMvxAsyncCommand CheckVersionCommand => new MvxAsyncCommand(this.CheckVersion);

        public IMvxCommand CancelUpgrade => new MvxCommand(() => this.cancellationTokenSource.Cancel());
        public IMvxAsyncCommand UpdateApplicationCommand => new MvxAsyncCommand(this.UpdateApplication);
        public IMvxCommand RejectUpdateApplicationCommand => new MvxCommand(RejectUpdateApplication);

        private readonly TimeSpan downloadApkTimeout = TimeSpan.FromMinutes(30);
        
        private async Task UpdateApplication()
        {
            this.IsNewVersionAvaliable = false;
            this.IsVersionCheckInProgress = true;
            this.cancellationTokenSource = new CancellationTokenSource(this.downloadApkTimeout);
            try
            {
                this.CheckNewVersionResult = InterviewerUIResources.Diagnostics_DownloadingPleaseWait;

                await this.tabletDiagnosticService.UpdateTheApp(this.cancellationTokenSource.Token,
                    progress =>
                    {
                        this.CheckNewVersionResult = InterviewerUIResources.Diagnostics_DownloadingPleaseWait 
                        + $" ({progress.ProgressPercentage}%)";
                    });

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
            this.cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var versionFromServer = await
                    this.synchronizationService.GetLatestApplicationVersionAsync(this.cancellationTokenSource.Token);

                if (!this.cancellationTokenSource.IsCancellationRequested)
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
    }
}