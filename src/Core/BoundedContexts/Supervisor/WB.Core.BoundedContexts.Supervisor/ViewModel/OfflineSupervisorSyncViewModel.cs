using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Logging;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    [ExcludeFromCodeCoverage()] // TODO: remove attribute when UI binding completed
    public class OfflineSupervisorSyncViewModel : BaseOfflineSyncViewModel
    {
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly string serviceName;

        public OfflineSupervisorSyncViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IPermissionsService permissions,
            IEnumeratorSettings settings,
            INearbyCommunicator communicator,
            INearbyConnection nearbyConnection,
            IInterviewViewModelFactory viewModelFactory)
            : base(principal, viewModelNavigationService, permissions, nearbyConnection)
        {
            communicator.IncomingInfo.Subscribe(OnIncomingData);
            this.viewModelFactory = viewModelFactory;
            this.serviceName = NormalizeEndpoint(settings.Endpoint);
        }

        private string title;
        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        private bool hasConnectedDevices = true;
        public bool HasConnectedDevices
        {
            get => this.hasConnectedDevices;
            set => this.SetProperty(ref this.hasConnectedDevices, value);
        }

        private bool allSynchronizationsFinished;
        public bool AllSynchronizationsFinished
        {
            get => this.allSynchronizationsFinished;
            set => this.SetProperty(ref this.allSynchronizationsFinished, value);
        }

        private string progressTitle;
        public string ProgressTitle
        {
            get => this.progressTitle;
            set => this.SetProperty(ref this.progressTitle, value);
        }

        private ObservableCollection<ConnectedDeviceViewModel> connectedDevices;
        public ObservableCollection<ConnectedDeviceViewModel> ConnectedDevices
        {
            get => this.connectedDevices;
            set => this.SetProperty(ref this.connectedDevices, value);
        }

        public override async Task Initialize()
        {
            this.Title = SupervisorUIResources.OfflineSync_ReceivingInterviewsFromDevices;
            this.ProgressTitle = string.Format(SupervisorUIResources.OfflineSync_NoDevicesDetectedFormat,
                this.principal.CurrentUserIdentity.Name);

            this.ConnectedDevices = new ObservableCollection<ConnectedDeviceViewModel>(new[]
            {
                this.viewModelFactory.GetNew<ConnectedDeviceViewModel>(),
                this.viewModelFactory.GetNew<ConnectedDeviceViewModel>()
            });

            await this.ConnectedDevices[0].Initialize();
            await this.ConnectedDevices[1].Initialize();
        }

        protected override void SetStatus(ConnectionStatus connectionStatus, string details = null)
        {
            base.SetStatus(connectionStatus, details);
            this.ProgressTitle = $"{this.GetServiceName()}\r\n{this.Status}\r\n{this.StatusDetails}";
        }

        public IMvxAsyncCommand Restart => new MvxAsyncCommand(OnGoogleApiReady);

        protected override async Task OnGoogleApiReady()
        {
            Log.Trace("StartAdvertising");

            StopAdvertising();
            await StartAdvertising();
        }

        private void OnIncomingData(IncomingDataInfo dataInfo)
        {
            SetStatus(ConnectionStatus.Sync, dataInfo.ToString());
        }
        
        protected override string GetServiceName() => serviceName + this.principal.CurrentUserIdentity.UserId.FormatGuid();
    }
}
