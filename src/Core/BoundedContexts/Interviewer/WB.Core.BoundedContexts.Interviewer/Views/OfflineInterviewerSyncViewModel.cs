using System;
using System.Threading.Tasks;
using Humanizer;
using MvvmCross.Logging;
using WB.Core.GenericSubdomains.Portable;
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
        private string status;
        private string statusDetails;

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
            SetStatus(ConnectionStatus.WaitingForGoogleApi);
        }

        public async Task OnGoogleApiReady()
        {
            SetStatus(ConnectionStatus.StartDiscovering);
            await StartDiscovery();
            SetStatus(ConnectionStatus.Discovering);
        }

        protected override string GetServiceName()
        {
            var user = this.interviewersPlainStorage.FirstOrDefault();
            return serviceName + user.SupervisorId;
        }

        // just to test that communication is working
        protected override async void OnConnectionResult(string endpoint, NearbyConnectionResolution resolution)
        {
            // SetStatus(ConnectionStatus.Sync, $"Connected to {endpoint} with status {resolution.IsSuccess}");
            base.OnConnectionResult(endpoint, resolution);

            if (resolution.IsSuccess)
            {
                Log.Info("Sending first request");
                var result = await syncClient.GetQuestionnaireList(endpoint);
                SetStatus(ConnectionStatus.Sync, $"Send first payload to {endpoint}");
                Log.Info("Sending second request");
                var res2 = await syncClient.GetQuestionnaireList(endpoint);
                SetStatus(ConnectionStatus.Sync, $"Send second payload to {endpoint}");
                Log.Info("Sending third request");
                var res3 = await syncClient.GetQuestionnaireList(endpoint);
                SetStatus(ConnectionStatus.Sync, $"Send third payload to {endpoint}");

                SetStatus(ConnectionStatus.Sync, $"Sending 20 Mb data {endpoint}");
                var data = new byte[1024 * 1024 * 20 /* 50 Mb*/];
                
                IProgress<CommunicationProgress> progress = new Progress<CommunicationProgress>(
                    p =>
                    {
                        SetStatus(ConnectionStatus.Sync, $"Sending {data.Length.Bytes().ToString("0.00")} data {endpoint} {p.TransferedBytes} of {p.TotalBytes}. " +
                                                         $"\r\nETA: {p.Eta:g} at {p.Speed}/s");
                    });
                var res4 = await syncClient.SendBigData(endpoint, data, progress);
            }
        }
    }
}
