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

namespace WB.Core.BoundedContexts.Interviewer.Views
{

    public class OfflineInterviewerSyncViewModel : BaseOfflineSyncViewModel, IOfflineSyncViewModel
    {
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IOfflineSyncClient syncClient;
        private readonly string serviceName;
        
        public OfflineInterviewerSyncViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService, 
            IPermissionsService permissions,
            IEnumeratorSettings settings,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            INearbyConnection nearbyConnection,
            IOfflineSyncClient syncClient) 
            : base(principal, viewModelNavigationService, permissions, nearbyConnection)
        {
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.syncClient = syncClient;

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

        protected override async void Connected(string connectedEndpoint)
        {
            var rng = new RNGCryptoServiceProvider();
            var data = new byte[10 * 1024 * 1024];
            rng.GetBytes(data);

            var response = await syncClient.SendAsync<SendBigAmountOfDataRequest, SendBigAmountOfDataResponse>(
                new SendBigAmountOfDataRequest()
                {
                    Data = data
                }, CancellationToken.None);
        }

        protected override string GetServiceName()
        {
            var user = this.interviewersPlainStorage.FirstOrDefault();
            return serviceName + user.SupervisorId;
        }

        //// just to test that communication is working
        //protected override async void OnConnectionResult(string endpoint, NearbyConnectionResolution resolution)
        //{
        //    // SetStatus(ConnectionStatus.Sync, $"Connected to {endpoint} with status {resolution.IsSuccess}");
        //    base.OnConnectionResult(endpoint, resolution);

        //    if (resolution.IsSuccess)
        //    {
               
        //    }
        //}
    }
}
