using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Logging;
using Plugin.Permissions.Abstractions;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    [ExcludeFromCodeCoverage()] // TODO: remove attribute when UI binding completed
    public class SupervisorOfflineSyncViewModel : BaseOfflineSyncViewModel, IOfflineSyncViewModel
    {
        private readonly IInterviewViewModelFactory viewModelFactory;

        private ReaderWriterLockSlim devicesLock = new ReaderWriterLockSlim();

        public SupervisorOfflineSyncViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IPermissionsService permissions,
            IEnumeratorSettings settings,
            INearbyCommunicator communicator,
            INearbyConnection nearbyConnection,
            IInterviewViewModelFactory viewModelFactory,
            IDeviceSynchronizationProgress deviceSynchronizationProgress)
            : base(principal, viewModelNavigationService, permissions, nearbyConnection, settings)
        {
            SetStatus(ConnectionStatus.WaitingForGoogleApi);
            communicator.IncomingInfo.Subscribe(OnIncomingData);
            this.viewModelFactory = viewModelFactory;
            devicesSubscribtion = deviceSynchronizationProgress.SyncStats.Subscribe(OnDeviceProgressReported);
        }

        private string title;
        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        private bool hasConnectedDevices = true;
        public bool HasConnectedDevices
        {
            get => this.hasConnectedDevices;
            set => this.SetProperty(ref this.hasConnectedDevices, value);
        }

        private bool allSynchronizationsFinished;
        public bool AllSynchronizationsFinished
        {
            get => this.allSynchronizationsFinished;
            set => this.SetProperty(ref this.allSynchronizationsFinished, value);
        }

        private string progressTitle;
        public string ProgressTitle
        {
            get => this.progressTitle;
            set => this.SetProperty(ref this.progressTitle, value);
        }

        private ObservableCollection<ConnectedDeviceViewModel> connectedDevices;
        private IDisposable devicesSubscribtion;

        public ObservableCollection<ConnectedDeviceViewModel> ConnectedDevices
        {
            get => this.connectedDevices;
            set => this.SetProperty(ref this.connectedDevices, value);
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            this.Title = SupervisorUIResources.OfflineSync_ReceivingInterviewsFromDevices;
            this.ProgressTitle = string.Format(SupervisorUIResources.OfflineSync_NoDevicesDetectedFormat,
                this.principal.CurrentUserIdentity.Name);
            this.connectedDevices = new ObservableCollection<ConnectedDeviceViewModel>();
        }

        private void SetStatus(ConnectionStatus connectionStatus, string details = null)
        {
            var newLine = Environment.NewLine;
            this.ProgressTitle = $"{this.GetServiceName()}{newLine}{connectionStatus.ToString()}{newLine}{details ?? String.Empty}";
        }

        protected void OnDeviceProgressReported(DeviceSyncStats stats)
        {
            SendingDeviceStatus deviceStatus;

            if (stats.ProgressInfo.IsRunning)
            {
                deviceStatus = SendingDeviceStatus.Synchronizing;
            }
            else
            {
                deviceStatus = stats.ProgressInfo.HasErrors ? SendingDeviceStatus.DoneWithErrors : SendingDeviceStatus.Done;
            }

            this.SetDeviceStatus(stats.InterviewerLogin, deviceStatus);

            var deviceInfo = FindDevice(stats);

            if (deviceInfo != null)
            {
                deviceInfo.Statistics = stats.ProgressInfo.Statistics;
            }
        }

        private ConnectedDeviceViewModel FindDevice(DeviceSyncStats stats)
        {
            this.devicesLock.EnterReadLock();

            ConnectedDeviceViewModel deviceInfo;
            try
            {
                deviceInfo = this.ConnectedDevices.FirstOrDefault(x => x.InterviewerName == stats.InterviewerLogin);
            }
            finally
            {
                this.devicesLock.ExitReadLock();
            }

            return deviceInfo;
        }

        protected override void OnDeviceFound(string name)
        {
           // Not called?
        }

        protected override void OnDeviceConnectionRequested(string name)
        {
            SetDeviceStatus(name, SendingDeviceStatus.ConnectionRequested);
        }

        protected override void OnDeviceConnected(string name)
        {
            this.devicesLock.EnterWriteLock();

            try
            {
                var existingDevice = this.ConnectedDevices.FirstOrDefault(x => x.InterviewerName == name);
                if (existingDevice == null)
                {
                    var newDevice = this.viewModelFactory.GetNew<ConnectedDeviceViewModel>();
                    newDevice.InterviewerName = name;
                    this.ConnectedDevices.Add(newDevice);
                }
                else
                {
                    existingDevice.Status = SendingDeviceStatus.Connected;
                }
            }
            finally
            {
                this.devicesLock.ExitWriteLock();
            }

            //SetDeviceStatus(name, SendingDeviceStatus.Connected);
        }

        protected override void OnDeviceDisconnected(string name)
        {
            SetDeviceStatus(name, SendingDeviceStatus.Disconnected);
        }

        private void SetDeviceStatus(string name, SendingDeviceStatus existingDeviceStatus)
        {
            this.devicesLock.EnterReadLock();

            try
            {
                var existingDevice = this.ConnectedDevices.FirstOrDefault(x => x.InterviewerName == name);
                if (existingDevice != null)
                {
                    existingDevice.Status = existingDeviceStatus;
                }
            }
            finally
            {
                this.devicesLock.ExitReadLock();
            }
        }

        public IMvxAsyncCommand StartDiscoveryAsyncCommand => new MvxAsyncCommand(() =>
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            return this.StartAdvertising();
        });

        private async Task StartAdvertising()
        {
            await this.permissions.AssureHasPermission(Permission.Location);

            Log.Trace("StartAdvertising");

            SetStatus(ConnectionStatus.StartAdvertising, $"Starting advertising");
            var serviceName = this.GetServiceName();
            try
            {
                await this.nearbyConnection.StartAdvertisingAsync(serviceName, this.principal.CurrentUserIdentity.Name, cancellationTokenSource.Token);
                SetStatus(ConnectionStatus.Advertising, "Waiting for interviewers connections");
            }
            catch (NearbyConnectionException nce)
            {
                SetStatus(ConnectionStatus.Error, nce.Message);
            }
        }

        private void OnIncomingData(IncomingDataInfo dataInfo)
        {
            SetStatus(ConnectionStatus.Sync, dataInfo.ToString());
        }

        protected override string GetDeviceIdentification() => this.principal.CurrentUserIdentity.UserId.FormatGuid();

        public IMvxCommand CancelCommand => new MvxCommand(() => { });

        public override void Dispose()
        {
            base.Dispose();
            this.devicesSubscribtion?.Dispose();
        }
    }
}
