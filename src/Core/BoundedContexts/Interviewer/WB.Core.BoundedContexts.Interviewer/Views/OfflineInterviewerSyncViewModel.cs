using System.Threading.Tasks;
using MvvmCross.Logging;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{

    public class OfflineInterviewerSyncViewModel : BaseOfflineSyncViewModel, IOfflineSyncViewModel
    {
        private readonly IOfflineSyncClient syncClient;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly string serviceName;

        public OfflineInterviewerSyncViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService, 
            IPermissionsService permissions, 
            IOfflineSyncClient syncClient,
            IEnumeratorSettings settings,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            INearbyConnection nearbyConnection) 
            : base(principal, viewModelNavigationService, permissions, nearbyConnection)
        {
            this.syncClient = syncClient;
            this.interviewersPlainStorage = interviewersPlainStorage;

            this.serviceName = settings.Endpoint + "/";
        }

        public InterviewerConnectionStatus Status { get; set; }
        
        public Task OnGoogleApiReady()
        {
            return StartDiscovery();
        }


        protected override string GetServiceName()
        {
            var user = this.interviewersPlainStorage.FirstOrDefault();
            return serviceName + user.SupervisorId;
        }

        // just to test that communication is working
        protected override async void OnConnectionResult(string endpoint, NearbyConnectionResolution resolution)
        {
            base.OnConnectionResult(endpoint, resolution);

            if (resolution.IsSuccess)
            {
                Log.Info("Sending first request");
                var result = await syncClient.GetQuestionnaireList(endpoint);
                Log.Info("Sending second request");
                var res2 = await syncClient.GetQuestionnaireList(endpoint);
                Log.Info("Sending third request");
                var res3 = await syncClient.GetQuestionnaireList(endpoint);

                Log.Info("Sending 50 Mb");
                var data = new byte[1024 * 1024 * 50 /* 50 Mb*/];

                var res4 = await syncClient.SendBigData(endpoint, data);
            }
        }
    }

    public enum InterviewerConnectionStatus
    {
        WaitingForGoogleApi,
        Discovering,
        Connecting,
        Sync,
        Done
    }
}
