using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class DiagnosticsViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ITabletDiagnosticService tabletDiagnosticService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly ISynchronizationService synchronizationService;
        private readonly IUserInteractionService userInteractionService;
        private readonly ILogger logger;

        private bool isRestoreVisible;
        private bool isVersionCheckInProgress;

        public DiagnosticsViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IInterviewerSettings interviewerSettings, 
            SendTabletInformationViewModel sendTabletInformationViewModel, 
            ISynchronizationService synchronizationService, 
            ILogger logger, IUserInteractionService userInteractionService, 
            ITabletDiagnosticService tabletDiagnosticService)
        {
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewerSettings = interviewerSettings;
            this.TabletInformation = sendTabletInformationViewModel;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.userInteractionService = userInteractionService;
            this.tabletDiagnosticService = tabletDiagnosticService;
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

        public bool IsVersionCheckInProgress
        {
            get { return this.isVersionCheckInProgress; }
            set { this.RaiseAndSetIfChanged(ref this.isVersionCheckInProgress, value); }
        }

        public string Version { get; set; }

        public SendTabletInformationViewModel TabletInformation { get; set; }

        public IMvxCommand ShareDeviceTechnicalInformationCommand => new MvxCommand(this.ShareDeviceTechnicalInformation);

        public IMvxCommand NavigateToDashboardCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToAsync<LoginViewModel>()); }
        }

        public IMvxCommand SignOutCommand
        {
            get { return new MvxCommand(async () => await this.SignOut()); }
        }

        public IMvxCommand NavigateToLoginCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToAsync<LoginViewModel>()); }
        }

        public IMvxCommand CheckVersionCommand
        {
            get { return new MvxCommand(async () => await this.CheckVersion()); }
        }

        public bool IsAuthenticated => this.principal.IsAuthenticated;

        private void ShareDeviceTechnicalInformation()
        {
            this.tabletDiagnosticService.LaunchShareAction(InterviewerUIResources.Share_to_Title,
                this.interviewerSettings.GetDeviceTechnicalInformation());
        }

        private async Task SignOut()
        {
            this.principal.SignOut();
            await this.viewModelNavigationService.NavigateToAsync<LoginViewModel>();
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