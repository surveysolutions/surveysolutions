using System;
using System.Diagnostics.CodeAnalysis;
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
        private readonly IPermissionsService permissions;
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
                    Connected(connected.Endpoint);
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

        protected virtual void Disconnected(string disconnectedEndpoint)
        {

        }

        protected virtual void SetStatus(ConnectionStatus connectionStatus, string details = null)
        {
            this.Status = connectionStatus.ToString();
            this.StatusDetails = details ?? String.Empty;
        }

        protected virtual void Connected(string connectedEndpoint)
        {

        }

        protected virtual async void OnFound(string endpointId, NearbyDiscoveredEndpointInfo info)
        {
            SetStatus(ConnectionStatus.Connecting, $"Found {info.EndpointName}. Requesting conection");
            Log.Trace("OnFound {0} - {1}", endpointId, info.EndpointName);
            var result =  await this.nearbyConnection.RequestConnection(this.principal.CurrentUserIdentity.Name, endpointId);

            if (!SetErrorOnResultIfNeeded(result)) return;

            SetStatus(ConnectionStatus.Connecting, $"Requested conection to {info.EndpointName}");
        }
        private string endpoint;
        public string Endpoint
        {
            get => endpoint;
            set => SetProperty(ref endpoint, value);
        }

        protected async Task StartDiscovery()
        {
            await permissions.AssureHasPermission(Permission.Location);

            SetStatus(ConnectionStatus.StartDiscovering, $"Starting discovery");
            var serviceName = GetServiceName();
            var result = await this.nearbyConnection.StartDiscovery(serviceName);

            if (!SetErrorOnResultIfNeeded(result)) return;

            Endpoint = serviceName;
            SetStatus(ConnectionStatus.Discovering, $"Searching for supervisor");
        }

        private bool SetErrorOnResultIfNeeded(NearbyStatus status)
        {
            if (status.IsSuccess == false)
            {
                SetStatus(ConnectionStatus.Error, status.StatusMessage + $" [{status.Status.ToString()}]");
            }

            return status.IsSuccess;
        }

        protected void StopDiscovery()
        {
            this.nearbyConnection.StopAllEndpoint();
            this.nearbyConnection.StopDiscovery();
        }

        protected void StopAdvertising()
        {
            this.nearbyConnection.StopAllEndpoint();
            this.nearbyConnection.StopAdvertising();
        }

        protected abstract string GetServiceName();

        protected async Task StartAdvertising()
        {
            await permissions.AssureHasPermission(Permission.Location);

            Log.Trace("StartAdvertising");

            SetStatus(ConnectionStatus.StartAdvertising, $"Starting advertising");
            var serviceName = GetServiceName();
            try
            {
                await this.nearbyConnection.StartAdvertising(serviceName, this.principal.CurrentUserIdentity.Name);
                Endpoint = serviceName;
                SetStatus(ConnectionStatus.Advertising, "Waiting for interviewers connections");
            }
            catch (NearbyConnectionException nce)
            {
                SetStatus(ConnectionStatus.Error, nce.Message);
            }
        }

        protected virtual async void OnConnection(string endpointId, NearbyConnectionInfo info)
        {
            SetStatus(ConnectionStatus.Connecting, $"New incoming connection from {info.EndpointName}");

            Log.Trace("OnConnection from " + endpointId + " => " + info.EndpointName);
            var result = await this.nearbyConnection.AcceptConnection(endpointId);
            if (!SetErrorOnResultIfNeeded(result)) return;

            Log.Trace("Accept connection from " + endpointId + " => " + info.EndpointName);

            SetStatus(ConnectionStatus.Connected, $"Connected to {info.EndpointName}");
        }

        private string status;
        private string statusDetails;

        public string Status
        {
            get => status;
            set => SetProperty(ref status, value);
        }

        public string StatusDetails
        {
            get => statusDetails;
            set => SetProperty(ref statusDetails, value);
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

        protected string NormalizeEndpoint(string endpoint)
        {
            var uri = new Uri(endpoint);
            return uri.ToString().TrimEnd('/').ToLower();
        }
    }
}
