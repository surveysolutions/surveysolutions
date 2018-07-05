using System.Threading.Tasks;
using MvvmCross.Logging;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class OfflineSupervisorSyncViewModel : BaseOfflineSyncViewModel, IOfflineSyncViewModel
    {
        private readonly IPlainStorage<SupervisorIdentity> supervisorStorage;
        private readonly string serviceName;

        public OfflineSupervisorSyncViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IPermissionsService permissions,
            IEnumeratorSettings settings,
            IPlainStorage<SupervisorIdentity> supervisorStorage,
            INearbyConnection nearbyConnection)
            : base(principal, viewModelNavigationService, permissions, nearbyConnection)
        {
            this.supervisorStorage = supervisorStorage;
            serviceName = settings.Endpoint + "/";// + identity.UserId;
        }

        public async Task OnGoogleApiReady()
        {
            Log.Trace("StartAdvertising");

            await StartAdvertising();
        }
        
        protected override string GetServiceName()
        {
            var sup = supervisorStorage.FirstOrDefault();
            return serviceName + sup.UserId;
        }
    }
}
