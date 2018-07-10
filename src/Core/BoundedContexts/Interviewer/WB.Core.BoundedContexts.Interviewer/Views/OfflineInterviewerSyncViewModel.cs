using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using MvvmCross.Commands;
using MvvmCross.Logging;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Interviewer.Views
{

    public class OfflineInterviewerSyncViewModel : BaseOfflineSyncViewModel, IOfflineSyncViewModel
    {
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly ISynchronizationProcess synchronizationProcess;
        private readonly ISynchronizationService synchronizationService;
        private readonly IOfflineSyncClient syncClient;
        private readonly string serviceName;
        
        public OfflineInterviewerSyncViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService, 
            IPermissionsService permissions,
            IEnumeratorSettings settings,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            INearbyConnection nearbyConnection,
            ISynchronizationProcess synchronizationProcess,
            ISynchronizationService synchronizationService) 
            : base(principal, viewModelNavigationService, permissions, nearbyConnection)
        {
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.synchronizationProcess = synchronizationProcess;
            this.synchronizationService = synchronizationService;


            this.serviceName = settings.Endpoint + "/";
            SetStatus(ConnectionStatus.WaitingForGoogleApi);
        }

        public async Task OnGoogleApiReady()
        {
            SetStatus(ConnectionStatus.StartDiscovering);
            await StopDiscovery();
            await StartDiscovery();
            SetStatus(ConnectionStatus.Discovering);
        }

        public IMvxAsyncCommand Restart => new MvxAsyncCommand(OnGoogleApiReady);
        public IMvxAsyncCommand Sync => new MvxAsyncCommand(Synchronize);

        public Task Synchronize()
        {
            return synchronizationProcess.SyncronizeAsync(new Progress<SyncProgressInfo>(o =>
                {
                    this.StatusDetails = o.Description + "\r\n" + this.StatusDetails;
                }), CancellationToken.None);
        }

        protected override string GetServiceName()
        {
            var user = this.interviewersPlainStorage.FirstOrDefault();
            return serviceName + user.SupervisorId;
        }
    }
}
