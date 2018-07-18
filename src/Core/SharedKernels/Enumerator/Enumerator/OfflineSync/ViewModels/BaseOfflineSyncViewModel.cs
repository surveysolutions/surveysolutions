using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MvvmCross.Logging;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels
{
    public enum ConnectionStatus
    {
        WaitingForGoogleApi,
        StartDiscovering,
        StartAdvertising,
        Discovering,
        Connecting,
        Sync,
        Done,
        Connected,
        Advertising,
        Error
    }

    [ExcludeFromCodeCoverage()] // TODO: remove attribute when UI binding completed
    public abstract class BaseOfflineSyncViewModel : BaseViewModel, IOfflineSyncViewModel
    {
        protected readonly IPermissionsService permissions;
        protected readonly INearbyConnection nearbyConnection;
        private readonly IEnumeratorSettings settings;

        protected BaseOfflineSyncViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IPermissionsService permissions,
            INearbyConnection nearbyConnection, IEnumeratorSettings settings)
            : base(principal, viewModelNavigationService)
        {
            this.permissions = permissions;
            this.nearbyConnection = nearbyConnection;
            this.settings = settings;
            SetStatus(ConnectionStatus.WaitingForGoogleApi);
            this.nearbyConnection.Events.Subscribe(HandleConnectionEvents);
        }

        protected void HandleConnectionEvents(INearbyEvent @event)
        {
            switch (@event)
            {
                case NearbyEvent.InitiatedConnection iniConnection:
                    this.nearbyConnection.AcceptConnection(iniConnection.Endpoint)
                        .ContinueWith(result => SetErrorOnResultIfNeeded(result.Result));
                    break;
                case NearbyEvent.Connected connected:
                    SetStatus(ConnectionStatus.Connected, "Connected to " + connected.Name);
                    Connected(connected.Endpoint, connected.Name);
                    break;
                case NearbyEvent.Disconnected disconnected:
                    SetStatus(ConnectionStatus.Discovering, "Disconnected from " + disconnected.Name ?? disconnected.Endpoint);
                    Disconnected(disconnected.Endpoint);
                    break;
                case NearbyEvent.EndpointFound endpointFound:
                    this.OnFound(endpointFound.Endpoint, endpointFound.EndpointInfo);
                    break;
                case NearbyEvent.EndpointLost endpointLost:
                    break;
            }
        }

        protected virtual void Disconnected(string disconnectedEndpoint) { }

        protected virtual void SetStatus(ConnectionStatus connectionStatus, string details = null) { }

        protected virtual void Connected(string connectedEndpoint, string connectedTo) { }

        protected virtual void OnError(string errorMessage, ConnectionStatusCode errorCode) { }

        protected virtual async void OnFound(string endpointId, NearbyDiscoveredEndpointInfo info)
        {
            SetStatus(ConnectionStatus.Connecting, $"Found {info.EndpointName}. Requesting conection");
            Log.Trace("OnFound {0} - {1}", endpointId, info.EndpointName);
            var result =  await this.nearbyConnection.RequestConnection(this.principal.CurrentUserIdentity.Name, endpointId);

            if (!SetErrorOnResultIfNeeded(result)) return;

            SetStatus(ConnectionStatus.Connecting, $"Requested conection to {info.EndpointName}");
        }

        protected bool SetErrorOnResultIfNeeded(NearbyStatus status)
        {
            if (status.IsSuccess == false)
            {
                this.OnError(status.StatusMessage, status.Status);
                SetStatus(ConnectionStatus.Error, status.StatusMessage + $" [{status.Status.ToString()}]");
            }

            return status.IsSuccess;
        }

        protected string GetServiceName()
        {
            var normalizedEndpoint = new Uri(this.settings.Endpoint).ToString().TrimEnd('/').ToLower();

            return $"{normalizedEndpoint}/{this.GetDeviceIdentification()}";
        }

        protected abstract string GetDeviceIdentification();

        protected virtual async void OnConnection(string endpointId, NearbyConnectionInfo info)
        {
            SetStatus(ConnectionStatus.Connecting, $"New incoming connection from {info.EndpointName}");

            Log.Trace("OnConnection from " + endpointId + " => " + info.EndpointName);
            var result = await this.nearbyConnection.AcceptConnection(endpointId);
            if (!SetErrorOnResultIfNeeded(result)) return;

            Log.Trace("Accept connection from " + endpointId + " => " + info.EndpointName);

            SetStatus(ConnectionStatus.Connected, $"Connected to {info.EndpointName}");
        }

        protected abstract Task OnGoogleApiReady();

        public void SetGoogleAwaiter(Task<bool> apiConnected)
        {
            if (apiConnected.IsCompleted)
            {
                this.OnGoogleApiReady();
            }
            else
            {
                apiConnected.ContinueWith(async res => this.OnGoogleApiReady());
            }
        }
    }
}
