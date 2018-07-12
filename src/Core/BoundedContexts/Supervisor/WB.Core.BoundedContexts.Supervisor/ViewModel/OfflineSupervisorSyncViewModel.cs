using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Logging;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class OfflineSupervisorSyncViewModel : BaseOfflineSyncViewModel
    {
        private readonly IPlainStorage<SupervisorIdentity> supervisorStorage;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly string serviceName;

        public OfflineSupervisorSyncViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IPermissionsService permissions,
            IEnumeratorSettings settings,
            IPlainStorage<SupervisorIdentity> supervisorStorage,
            INearbyCommunicator communicator,
            INearbyConnection nearbyConnection,
            IInterviewViewModelFactory viewModelFactory)
            : base(principal, viewModelNavigationService, permissions, nearbyConnection)
        {
            communicator.IncomingInfo.Subscribe(OnIncomingData);
            this.supervisorStorage = supervisorStorage;
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

        public override void Prepare()
        {
            base.Prepare();

            this.Title = SupervisorUIResources.OfflineSync_ReceivingInterviewsFromDevices;
            this.ProgressTitle = string.Format(SupervisorUIResources.OfflineSync_NoDevicesDetectedFormat,
                this.principal.CurrentUserIdentity.Name);

            var vm1 = this.viewModelFactory.GetNew<ConnectedDeviceViewModel>();
            vm1.Initialize().WaitAndUnwrapException();
            var vm2 = this.viewModelFactory.GetNew<ConnectedDeviceViewModel>();
            vm2.Initialize().WaitAndUnwrapException();
            this.ConnectedDevices = new ObservableCollection<ConnectedDeviceViewModel>(new[]
            {
                vm1,
                vm2
            });
        }

        public IMvxAsyncCommand Restart => new MvxAsyncCommand(OnGoogleApiReady);

        protected override async Task OnGoogleApiReady()
        {
            Log.Trace("StartAdvertising");

            await StopAdvertising();
            await StartAdvertising();
        }

        private void OnIncomingData(IncomingDataInfo dataInfo)
        {
            SetStatus(ConnectionStatus.Sync, dataInfo.ToString());
        }
        
        protected override string GetServiceName()
        {
            var sup = supervisorStorage.FirstOrDefault();
            return serviceName + sup.UserId.FormatGuid();
        }
    }
}
