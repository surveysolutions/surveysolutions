using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class DiagnosticsViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ITabletDiagnosticService tabletDiagnosticService;
        private readonly IInterviewerSettings interviewerSettings;

        public DiagnosticsViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IInterviewerSettings interviewerSettings, 
            ITabletDiagnosticService tabletDiagnosticService,
            BackupViewModel sendTabletInformationViewModel,
            CheckNewVersionViewModel checkNewVersion,
            BackupRestoreViewModel backupRestore,
            BandwidthTestViewModel bandwidthTest) : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewerSettings = interviewerSettings;
            this.tabletDiagnosticService = tabletDiagnosticService;
            this.TabletInformation = sendTabletInformationViewModel;
            this.CheckNewVersion = checkNewVersion;
            this.BackupRestore = backupRestore;
            this.BandwidthTest = bandwidthTest;
        }

        public override bool IsAuthenticationRequired => false;

        public BackupViewModel TabletInformation { get; set; }

        public CheckNewVersionViewModel CheckNewVersion { get; set; }

        public BackupRestoreViewModel BackupRestore { get; set; }

        public BandwidthTestViewModel BandwidthTest { get; set; }

        public IMvxCommand ShareDeviceTechnicalInformationCommand => new MvxCommand(this.ShareDeviceTechnicalInformation);

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () => await this.viewModelNavigationService.NavigateToDashboard());

        public IMvxCommand SignOutCommand
            => new MvxCommand(this.viewModelNavigationService.SignOutAndNavigateToLogin);

        public IMvxCommand NavigateToLoginCommand
            => new MvxCommand(this.viewModelNavigationService.NavigateToLogin);

        public bool IsAuthenticated => this.principal.IsAuthenticated;

        private void ShareDeviceTechnicalInformation()
        {
            this.tabletDiagnosticService.LaunchShareAction(InterviewerUIResources.Share_to_Title,
                this.interviewerSettings.GetDeviceTechnicalInformation());
        }
    }
}