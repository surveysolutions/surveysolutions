using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class DiagnosticsViewModel : BasePrincipalViewModel
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
            SendLogsViewModel logsViewModel) : base(principal, viewModelNavigationService, false)
        {
            this.deviceSettings = deviceSettings;
            this.tabletDiagnosticService = tabletDiagnosticService;
            this.logsViewModel = logsViewModel;
            this.TabletInformation = sendTabletInformationViewModel;
            this.CheckNewVersion = checkNewVersion;
            this.BackupRestore = backupRestore;
            this.BandwidthTest = bandwidthTest;
        }

        public SendTabletInformationViewModel TabletInformation { get; set; }

        public CheckNewVersionViewModel CheckNewVersion { get; set; }

        public BackupRestoreViewModel BackupRestore { get; set; }

        public BandwidthTestViewModel BandwidthTest { get; set; }

        public SendLogsViewModel Logs => logsViewModel;

        public IMvxCommand ShareDeviceTechnicalInformationCommand => new MvxCommand(this.ShareDeviceTechnicalInformation);
        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () => await this.ViewModelNavigationService.NavigateToDashboardAsync());
        public IMvxCommand NavigateToMapsCommand => new MvxAsyncCommand(this.ViewModelNavigationService.NavigateToMapsAsync);

        public IMvxCommand SignOutCommand
            => new MvxAsyncCommand(this.ViewModelNavigationService.SignOutAndNavigateToLoginAsync);

        public IMvxCommand NavigateToLoginCommand
            => new MvxAsyncCommand(this.ViewModelNavigationService.NavigateToLoginAsync);

        public bool IsAuthenticated => this.Principal.IsAuthenticated;

        private void ShareDeviceTechnicalInformation()
        {
            this.tabletDiagnosticService.LaunchShareAction(EnumeratorUIResources.Share_to_Title,
                this.deviceSettings.GetDeviceTechnicalInformation());
        }
    }
}
