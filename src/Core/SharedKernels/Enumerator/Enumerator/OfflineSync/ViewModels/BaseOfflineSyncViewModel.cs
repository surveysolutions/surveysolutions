using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels
{
    [ExcludeFromCodeCoverage()] // TODO: remove attribute when UI binding completed
    public abstract class BaseOfflineSyncViewModel : BaseViewModel, IDisposable
    {
        protected readonly IPermissionsService permissions;
        protected readonly INearbyConnection nearbyConnection;
        protected CancellationTokenSource cancellationTokenSource = null;
        private readonly IEnumeratorSettings settings;
        private readonly IDisposable nearbyConnectionSubscribtion;

        protected BaseOfflineSyncViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IPermissionsService permissions,
            INearbyConnection nearbyConnection, IEnumeratorSettings settings)
            : base(principal, viewModelNavigationService)
        {
            this.permissions = permissions;
            this.nearbyConnection = nearbyConnection;
            this.settings = settings;
            nearbyConnectionSubscribtion = this.nearbyConnection.Events.Subscribe(HandleConnectionEvents);
        }

        protected async void HandleConnectionEvents(INearbyEvent @event)
        {
            switch (@event)
            {
                case NearbyEvent.EndpointFound endpointFound:
                    await this.RequestConnectionAsync(endpointFound.Endpoint, endpointFound.EndpointInfo.EndpointName).ConfigureAwait(false);
                    break;
                case NearbyEvent.InitiatedConnection iniConnection:
                    await this.InitializeConnectionAsync(iniConnection.Endpoint, iniConnection.Info.EndpointName).ConfigureAwait(false);
                    break;
                case NearbyEvent.Connected connected:
                    this.OnDeviceConnected(connected.Name);
                    break;
                case NearbyEvent.Disconnected disconnected:
                    this.OnDeviceDisconnected(disconnected.Name);
                    break;
                case NearbyEvent.EndpointLost endpointLost:
                    break;
            }
        }

        private async Task InitializeConnectionAsync(string endpoint, string name)
        {
            this.OnDeviceConnectionAccepting(name);
            var connectionStatus = await this.nearbyConnection.AcceptConnectionAsync(endpoint)
                                             .ConfigureAwait(false);

            if (!connectionStatus.IsSuccess)
                this.OnConnectionError(connectionStatus.StatusMessage, connectionStatus.Status);
            else
                this.OnDeviceConnectionAccepted(name);
        }

        private async Task RequestConnectionAsync(string endpoint, string name)
        {
            this.OnDeviceFound(name);

            var connectionStatus = await this.nearbyConnection.RequestConnectionAsync(this.principal.CurrentUserIdentity.Name, endpoint, cancellationTokenSource.Token)
                                             .ConfigureAwait(false);

            if (!connectionStatus.IsSuccess)
                this.OnConnectionError(connectionStatus.StatusMessage, connectionStatus.Status);
            else
                this.OnDeviceConnectionRequested(name);
        }

        protected virtual void OnDeviceFound(string name) { }
        protected virtual void OnDeviceConnectionRequested(string name) { }
        protected virtual void OnDeviceConnectionAccepting(string name) { }
        protected virtual void OnDeviceConnectionAccepted(string name) { }
        protected virtual void OnDeviceConnected(string name) { }
        protected virtual void OnDeviceDisconnected(string name) { }
        protected virtual void OnConnectionError(string errorMessage, ConnectionStatusCode errorCode) { }

        protected string GetServiceName()
        {
            var normalizedEndpoint = new Uri(this.settings.Endpoint).ToString().TrimEnd('/').ToLower();

            return $"{normalizedEndpoint}/{this.GetDeviceIdentification()}";
        }

        protected abstract string GetDeviceIdentification();

        public virtual void Dispose()
        {
            cancellationTokenSource?.Dispose();
            nearbyConnectionSubscribtion?.Dispose();
        }
    }
}
