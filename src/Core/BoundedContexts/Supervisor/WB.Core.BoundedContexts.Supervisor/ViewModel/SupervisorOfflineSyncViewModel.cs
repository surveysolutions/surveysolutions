﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    [ExcludeFromCodeCoverage()] // TODO: remove attribute when UI binding completed
    public class SupervisorOfflineSyncViewModel : BaseOfflineSyncViewModel<object>, IOfflineSyncViewModel
    {
        public IUserInteractionService UserInteractionService { get; }
        public IGoogleApiService GoogleApiService { get; }
        private readonly IInterviewViewModelFactory viewModelFactory;

        private readonly ReaderWriterLockSlim devicesLock = new ReaderWriterLockSlim();

        public SupervisorOfflineSyncViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IPermissionsService permissions,
            INearbyCommunicator communicator,
            INearbyConnection nearbyConnection,
            IInterviewViewModelFactory viewModelFactory,
            IDeviceSynchronizationProgress deviceSynchronizationProgress,
            IUserInteractionService userInteractionService,
            IGoogleApiService googleApiService)
            : base(principal, viewModelNavigationService, permissions, nearbyConnection)
        {
            UserInteractionService = userInteractionService;
            GoogleApiService = googleApiService;
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
        private readonly IDisposable devicesSubscribtion;
        private bool isInitialized = false;

        public ObservableCollection<ConnectedDeviceViewModel> ConnectedDevices
        {
            get => this.connectedDevices;
            set => this.SetProperty(ref this.connectedDevices, value);
        }

        public override void Prepare(object parameter)
        {
            
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            this.Title = SupervisorUIResources.OfflineSync_ReceivingInterviewsFromDevices;

            this.connectedDevices = new ObservableCollection<ConnectedDeviceViewModel>();
            await StartDiscoveryAsyncCommand.ExecuteAsync();
        }

        private void SetStatus(ConnectionStatus connectionStatus, string details = null)
        {
            var newLine = Environment.NewLine;
            this.ProgressTitle = $"{(this.isInitialized ? this.GetServiceName() : string.Empty)}{newLine}{connectionStatus.ToString()}{newLine}{details ?? String.Empty}";
        }

        protected override void OnConnectionError(string errorMessage, ConnectionStatusCode errorCode)
        {
            switch (errorCode)
            {
                case ConnectionStatusCode.MissingPermissionAccessCoarseLocation:
                case ConnectionStatusCode.StatusEndpointUnknown:
                    SetStatus(ConnectionStatus.Error, errorMessage);
                    break;
            }
        }

        protected void OnDeviceProgressReported(DeviceSyncStats stats)
        {
            ConnectedDeviceViewModel deviceInfo = FindDevice(stats.InterviewerLogin);

            deviceInfo?.Synchronization.ProgressOnProgressChanged(this, stats.ProgressInfo);
        }

        private ConnectedDeviceViewModel FindDevice(string interviewerLogin)
        {
            this.devicesLock.EnterReadLock();

            ConnectedDeviceViewModel deviceInfo;
            try
            {
                deviceInfo = this.ConnectedDevices.FirstOrDefault(x => x.InterviewerName == interviewerLogin);
            }
            finally
            {
                this.devicesLock.ExitReadLock();
            }

            return deviceInfo;
        }

        protected override void OnDeviceFound(string name)
        {
            var device = FindDevice(name);
            if (device != null)
            {
                device.DeviceStatus = SendingDeviceStatus.Found;
            }
        }

        protected override void OnDeviceConnectionRequested(string name)
        {
            var device = FindDevice(name);
            if (device != null)
            {
                device.DeviceStatus = SendingDeviceStatus.ConnectionRequested;
            }
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
                    existingDevice.DeviceStatus = SendingDeviceStatus.Connected;
                }
            }
            finally
            {
                this.devicesLock.ExitWriteLock();
            }
        }

        protected override void OnDeviceDisconnected(string name)
        {
            var device = FindDevice(name);
            if (device != null)
            {
                device.DeviceStatus = SendingDeviceStatus.Disconnected;
            }
        }

        public IMvxAsyncCommand RetryCommand => new MvxAsyncCommand(() =>
        {
            ShouldStartAdvertising = true;
            return StartDiscoveryAsyncCommand.ExecuteAsync();
        });

        public IMvxAsyncCommand ExitOfflineSyncCommand => new MvxAsyncCommand(async () =>
            await this.viewModelNavigationService.NavigateToDashboardAsync());

        public IMvxAsyncCommand GoToDashboardCommand => new MvxAsyncCommand(() 
            => viewModelNavigationService.NavigateToDashboardAsync());

        protected override async Task OnStartDiscovery()
        {
            this.isInitialized = true;

            this.cancellationTokenSource = new CancellationTokenSource();

            SetStatus(ConnectionStatus.StartAdvertising, $"Starting advertising");

            var serviceName = this.GetServiceName();
            try
            {
                await this.nearbyConnection.StartAdvertisingAsync(serviceName, this.Principal.CurrentUserIdentity.Name,
                    cancellationTokenSource.Token);
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

        protected override string GetDeviceIdentification() => this.Principal.CurrentUserIdentity.UserId.FormatGuid();

        public IMvxCommand CancelCommand => new MvxCommand(() => { });

        public override void Dispose()
        {
            base.Dispose();
            this.devicesSubscribtion?.Dispose();
        }
    }
}
