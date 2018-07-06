using System;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
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
        private readonly string serviceName;
        private string status;
        private string statusDetails;

        public OfflineInterviewerSyncViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService, 
            IPermissionsService permissions,
            IEnumeratorSettings settings,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            INearbyConnection nearbyConnection) 
            : base(principal, viewModelNavigationService, permissions, nearbyConnection)
        {
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
               
            }
        }
    }
}
