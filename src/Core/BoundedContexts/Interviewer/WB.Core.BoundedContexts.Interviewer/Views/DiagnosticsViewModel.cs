using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
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
        private readonly IPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ITabletDiagnosticService tabletDiagnosticService;
        private readonly IInterviewerSettings interviewerSettings;

        public DiagnosticsViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IInterviewerSettings interviewerSettings, 
            ITabletDiagnosticService tabletDiagnosticService,
            SendTabletInformationViewModel sendTabletInformationViewModel,
            CheckNewVersionViewModel checkNewVersion,
            BackupRestoreViewModel backupRestore,
            BandwidthTestViewModel bandwidthTest)
        {
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewerSettings = interviewerSettings;
            this.tabletDiagnosticService = tabletDiagnosticService;
            this.TabletInformation = sendTabletInformationViewModel;
            this.CheckNewVersion = checkNewVersion;
            this.BackupRestore = backupRestore;
            this.BandwidthTest = bandwidthTest;
        }

        public SendTabletInformationViewModel TabletInformation { get; set; }

        public CheckNewVersionViewModel CheckNewVersion { get; set; }

        public BackupRestoreViewModel BackupRestore { get; set; }

        public BandwidthTestViewModel BandwidthTest { get; set; }

        public IMvxCommand ShareDeviceTechnicalInformationCommand => new MvxCommand(this.ShareDeviceTechnicalInformation);

        public IMvxCommand NavigateToDashboardCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToAsync<DashboardViewModel>()); }
        }

        public IMvxCommand SignOutCommand
        {
            get { return new MvxCommand(async () => await this.SignOut()); }
        }

        public IMvxCommand NavigateToLoginCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToAsync<LoginViewModel>()); }
        }

        public bool IsAuthenticated => this.principal.IsAuthenticated;

        private void ShareDeviceTechnicalInformation()
        {
            this.tabletDiagnosticService.LaunchShareAction(InterviewerUIResources.Share_to_Title,
                this.interviewerSettings.GetDeviceTechnicalInformation());
        }

        private async Task SignOut()
        {
            await this.principal.SignOutAsync();
            await this.viewModelNavigationService.NavigateToAsync<LoginViewModel>();
        }
    }
}