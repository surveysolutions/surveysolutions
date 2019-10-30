using MvvmCross.Commands;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class DiagnosticsViewModel : BaseViewModel
    {
        private readonly ITabletDiagnosticService tabletDiagnosticService;
        private readonly SendLogsViewModel logsViewModel;
        private readonly IDeviceSettings deviceSettings;

        public DiagnosticsViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IDeviceSettings deviceSettings, 
            ITabletDiagnosticService tabletDiagnosticService,
            SendTabletInformationViewModel sendTabletInformationViewModel,
            CheckNewVersionViewModel checkNewVersion,
            BackupRestoreViewModel backupRestore,
            BandwidthTestViewModel bandwidthTest,
            SendLogsViewModel logsViewModel) : base(principal, viewModelNavigationService)
        {
            this.deviceSettings = deviceSettings;
            this.tabletDiagnosticService = tabletDiagnosticService;
            this.logsViewModel = logsViewModel;
            this.TabletInformation = sendTabletInformationViewModel;
            this.CheckNewVersion = checkNewVersion;
            this.BackupRestore = backupRestore;
            this.BandwidthTest = bandwidthTest;
        }

        public override bool IsAuthenticationRequired => false;

        public SendTabletInformationViewModel TabletInformation { get; set; }

        public CheckNewVersionViewModel CheckNewVersion { get; set; }

        public BackupRestoreViewModel BackupRestore { get; set; }

        public BandwidthTestViewModel BandwidthTest { get; set; }

        public SendLogsViewModel Logs => logsViewModel;

        public IMvxCommand ShareDeviceTechnicalInformationCommand => new MvxCommand(this.ShareDeviceTechnicalInformation);
        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () => await this.viewModelNavigationService.NavigateToDashboardAsync());
        public IMvxCommand NavigateToMapsCommand => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToMapsAsync);

        public IMvxCommand SignOutCommand
            => new MvxAsyncCommand(this.viewModelNavigationService.SignOutAndNavigateToLoginAsync);

        public IMvxCommand NavigateToLoginCommand
            => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToLoginAsync);

        public bool IsAuthenticated => this.principal.IsAuthenticated;

        private void ShareDeviceTechnicalInformation()
        {
            this.tabletDiagnosticService.LaunchShareAction(EnumeratorUIResources.Share_to_Title,
                this.deviceSettings.GetDeviceTechnicalInformation());
        }
    }
}
