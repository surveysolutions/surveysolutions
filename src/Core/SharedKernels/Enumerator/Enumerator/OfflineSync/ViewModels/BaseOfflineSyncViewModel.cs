using System.Threading.Tasks;
using MvvmCross.Logging;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels
{
    public abstract class BaseOfflineSyncViewModel : BaseViewModel
    {
        private readonly IPermissionsService permissions;
        private readonly IEnumeratorSettings settings;
        private readonly INearbyConnection nearbyConnection;

        protected BaseOfflineSyncViewModel(
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IPermissionsService permissions,
            INearbyConnection nearbyConnection
        )
            : base(principal, viewModelNavigationService)
        {
            this.permissions = permissions;
            this.nearbyConnection = nearbyConnection;
        }

        private void OnLost(string endpoint)
        {
            Log.Trace("OnLost {0}", endpoint);
        }

        private async void OnFound(string endpoint, NearbyDiscoveredEndpointInfo info)
        {
            Log.Trace("OnFound {0} - {1}", endpoint, info.EndpointName);
            await this.nearbyConnection.RequestConnection(this.principal.CurrentUserIdentity.Name, endpoint,
                new NearbyConnectionLifeCycleCallback(OnInitiatedConnection, OnConnectionResult, OnDisconnected));
        }

        private void OnDisconnected(string endpoint)
        {
            Log.Trace("OnDisconnected {0}", endpoint);
            // stop all network activity
        }

        protected virtual void OnConnectionResult(string endpoint, NearbyConnectionResolution resolution)
        {
            Log.Trace("OnConnectionResult {0}, Success: {1}, Code: {2}", endpoint, resolution.IsSuccess, resolution.StatusCode);
        
        }

        protected virtual async void OnInitiatedConnection(string endpoint, NearbyConnectionInfo info)
        {
            Log.Trace("OnInitiatedConnection {0} - {1}", endpoint, info.EndpointName);
            await this.nearbyConnection.AcceptConnection(endpoint);
        }
        

        protected async Task StartDiscovery()
        {
            await permissions.AssureHasPermission(Permission.Location);

            await this.nearbyConnection.StartDiscovery(GetServiceName(), OnFound, OnLost);
        }

        protected abstract string GetServiceName();

        protected async Task StartAdvertising()
        {
            await permissions.AssureHasPermission(Permission.Location);

            Log.Trace("StartAdvertising");
            
            var res = await this.nearbyConnection.StartAdvertising(GetServiceName(), this.principal.CurrentUserIdentity.Name,
                new NearbyConnectionLifeCycleCallback(
                    OnConnection,
                    OnConnectionResult,
                    OnDisconnected));
        }

        private async void OnConnection(string endpoint, NearbyConnectionInfo info)
        {
            Log.Trace("OnConnection from " + endpoint + " => " + info.EndpointName);
            await this.nearbyConnection.AcceptConnection(endpoint);
            Log.Trace("Accept connection from " + endpoint + " => " + info.EndpointName);
        }
    }
}
