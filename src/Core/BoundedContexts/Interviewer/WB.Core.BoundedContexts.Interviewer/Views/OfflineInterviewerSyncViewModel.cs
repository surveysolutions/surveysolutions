using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class OfflineInterviewerSyncViewModel : BaseOfflineSyncViewModel
    {
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly ISynchronizationMode synchronizationMode;
        private readonly ISynchronizationCompleteSource synchronizationCompleteSource;
        private readonly string serviceName;
        
        public OfflineInterviewerSyncViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IPermissionsService permissions,
            IEnumeratorSettings settings,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            ISynchronizationMode synchronizationMode,
            INearbyConnection nearbyConnection,
            ISynchronizationCompleteSource synchronizationCompleteSource)
            : base(principal, viewModelNavigationService, permissions, nearbyConnection)
        {
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.synchronizationMode = synchronizationMode;
            this.synchronizationCompleteSource = synchronizationCompleteSource;

            this.serviceName = NormalizeEndpoint(settings.Endpoint);
            SetStatus(ConnectionStatus.WaitingForGoogleApi);
        }

        
        protected override async Task OnGoogleApiReady()
        {
            SetStatus(ConnectionStatus.StartDiscovering);
            await StopDiscovery();
            await StartDiscovery();
            SetStatus(ConnectionStatus.Discovering);
        }

        public IMvxAsyncCommand Restart => new MvxAsyncCommand(OnGoogleApiReady);
        public IMvxAsyncCommand Sync => new MvxAsyncCommand(Synchronize, () => CanConnect);

        public async Task Synchronize()
        {
            try
            {
                using (new CommunicationSession())
                {
                    this.synchronizationMode.Set(SynchronizationMode.Offline);
                    var synchronizationProcess = Mvx.Resolve<ISynchronizationProcess>();

                    await synchronizationProcess.SynchronizeAsync(
                        new Progress<SyncProgressInfo>(o =>
                        {
                            SetStatus(ConnectionStatus.Sync, o.Description);
                        }),
                        CancellationToken.None);

                    this.synchronizationCompleteSource.NotifyOnCompletedSynchronization(true);
                }
            }
            finally
            {
                this.synchronizationMode.Set(SynchronizationMode.Online);
            }
        }

        protected override void Connected(string connectedEndpoint)
        {
            this.CanConnect = true;
        }

        protected override void Disconnected(string endpoint)
        {
            this.CanConnect = false;
        }

        protected override string GetServiceName()
        {
            var user = this.interviewersPlainStorage.FirstOrDefault();
            return serviceName + user.SupervisorId.FormatGuid();
        }

        private bool canConnect;
        public bool CanConnect
        {
            get => canConnect;
            set
            {
                SetProperty(ref canConnect, value);
                this.RaisePropertyChanged(nameof(Sync));
            }
        }
    }
}
