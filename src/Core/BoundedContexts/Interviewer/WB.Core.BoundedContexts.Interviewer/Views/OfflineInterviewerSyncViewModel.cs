using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Bytes;
using Humanizer.Localisation;
using MvvmCross.Commands;
using Plugin.Permissions.Abstractions;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;
using InterviewerUIResources = WB.Core.BoundedContexts.Interviewer.Properties.InterviewerUIResources;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class OfflineInterviewerSyncViewModel : BaseOfflineSyncViewModel, IOfflineSyncViewModel
    {
        private static object syncLockObject = new Object();

        private readonly IInterviewerPrincipal principal;
        private readonly ISynchronizationMode synchronizationMode;
        private readonly ISynchronizationCompleteSource synchronizationCompleteSource;
        private readonly IOfflineSyncClient syncClient;
        private static CancellationTokenSource synchronizationCancellationTokenSource;

        public OfflineInterviewerSyncViewModel(IInterviewerPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IPermissionsService permissions,
            IEnumeratorSettings settings,
            ISynchronizationMode synchronizationMode,
            INearbyConnection nearbyConnection,
            ISynchronizationCompleteSource synchronizationCompleteSource,
            IOfflineSyncClient syncClient,
            IRestService restService)
            : base(principal, viewModelNavigationService, permissions, nearbyConnection, settings, restService)
        {
            this.principal = principal;
            this.synchronizationMode = synchronizationMode;
            this.synchronizationCompleteSource = synchronizationCompleteSource;
            this.syncClient = syncClient;
        }

        private string title;
        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        private string progressTitle;
        public string ProgressTitle
        {
            get => this.progressTitle;
            set => this.SetProperty(ref this.progressTitle, value);
        }

        private bool showNote;
        public bool ShowNote
        {
            get => this.showNote;
            set => this.SetProperty(ref this.showNote, value);
        }

        private string progressStatus;
        public string ProgressStatus
        {
            get => this.progressStatus;
            set => this.SetProperty(ref this.progressStatus, value);
        }

        private string progressStatusDescription;
        public string ProgressStatusDescription
        {
            get => this.progressStatusDescription;
            set => this.SetProperty(ref this.progressStatusDescription, value);
        }

        private decimal progressInPercents;
        public decimal ProgressInPercents
        {
            get => this.progressInPercents;
            set => this.SetProperty(ref this.progressInPercents, value);
        }

        private string networkInfo;
        public string NetworkInfo
        {
            get => this.networkInfo;
            set => this.SetProperty(ref this.networkInfo, value);
        }

        private string connectionStatus;
        public string ConnectionStatus
        {
            get => this.connectionStatus;
            set => this.SetProperty(ref this.connectionStatus, value);
        }

        private TransferingStatus transferingStatus;
        public TransferingStatus TransferingStatus
        {
            get => this.transferingStatus;
            set => this.SetProperty(ref this.transferingStatus, value);
        }

        public override Task Initialize()
        {
            this.BindTitlesAndStatuses();

            return base.Initialize();
        }

        private void BindTitlesAndStatuses()
        {
            this.TransferingStatus = TransferingStatus.WaitingDevice;
            this.Title = InterviewerUIResources.SendToSupervisor_MovingToSupervisorDevice;
            this.ProgressTitle = InterviewerUIResources.SendToSupervisor_CheckSupervisorDevice;
            this.ConnectionStatus = InterviewerUIResources.SendToSupervisor_LookingForSupervisor;
        }

        public IMvxCommand CancelCommand => new MvxCommand(Cancel);

        public IMvxCommand AbortCommand => new MvxCommand(Abort);
        public IMvxCommand DoneCommand => new MvxCommand(Done);

        public IMvxAsyncCommand RetryCommand => new MvxAsyncCommand(Retry);

        private void Done() => this.viewModelNavigationService.NavigateToDashboardAsync();
        private void Cancel()
        {
            this.StopDiscovery();
            this.Done();
        }
        private void Abort()
        {
            if (!synchronizationCancellationTokenSource?.IsCancellationRequested == true)
                synchronizationCancellationTokenSource.Cancel();

            this.ProgressTitle = InterviewerUIResources.SendToSupervisor_TransferWasAborted;
            this.TransferingStatus = TransferingStatus.Aborted;

            this.StopDiscovery();
        }

        private async Task Retry()
        {
            this.BindTitlesAndStatuses();
            this.nearbyConnection.StopAll();
            await this.StartDiscoveryAsyncCommand.ExecuteAsync();
        }
        
        protected override async Task OnStartDiscovery()
        {
            this.cancellationTokenSource?.Cancel();
            this.cancellationTokenSource = new CancellationTokenSource();
            await this.permissions.AssureHasPermission(Permission.Location);

            var discoveryStatus = await this.nearbyConnection.StartDiscoveryAsync(this.GetServiceName(), cancellationTokenSource.Token);
            if (!discoveryStatus.IsSuccess)
                this.OnConnectionError(discoveryStatus.StatusMessage, discoveryStatus.Status);
        }

        private void StopDiscovery() => this.nearbyConnection.StopDiscovery();

        protected override async void OnDeviceConnected(string name)
        {
            this.ProgressTitle = InterviewerUIResources.SendToSupervisor_TransferInProgress;
            this.TransferingStatus = TransferingStatus.Transferring;

            await this.SynchronizeAsync().ConfigureAwait(false);
        }

        protected override void OnDeviceDisconnected(string name)
        {
            if (new[] {TransferingStatus.Completed, TransferingStatus.Aborted}.Contains(this.TransferingStatus)) return;

            this.OnTerminateTransferring();
        }

        private void OnTerminateTransferring()
        {
            this.ProgressTitle = InterviewerUIResources.SendToSupervisor_SupervisorTerminateTransfering;
            this.TransferingStatus = TransferingStatus.CompletedWithErrors;
        }

        protected override void OnConnectionError(string errorMessage, ConnectionStatusCode errorCode)
        {
            this.StopDiscovery();

            switch (errorCode)
            {
                case ConnectionStatusCode.StatusBluetoothError:
                    this.ProgressTitle = InterviewerUIResources.SendToSupervisor_BluetoothError;
                    break;
                case ConnectionStatusCode.StatusEndpointUnknown:
                    this.ProgressTitle = errorMessage;
                    break;
                default:
                    this.ProgressTitle = InterviewerUIResources.SendToSupervisor_SupervisorNotFound;
                    break;
            }

            this.TransferingStatus = TransferingStatus.Failed;
        }

        protected override void OnDeviceFound(string name)
        {
            this.Title = string.Format(InterviewerUIResources.SendToSupervisor_MovingToSupervisorFormat, name);
            this.ConnectionStatus = InterviewerUIResources.SendToSupervisor_DeviceFound;
        }

        protected override void OnDeviceConnectionAccepting(string name)
            => this.ConnectionStatus = InterviewerUIResources.SendToSupervisor_DeviceConnectionAccepting;

        protected override void OnDeviceConnectionAccepted(string name)
            => this.ConnectionStatus = InterviewerUIResources.SendToSupervisor_DeviceConnectionAccepted;

        private void OnComplete(int failedInterviewsCount)
        {
            this.StopDiscovery();

            if (this.TransferingStatus == TransferingStatus.Aborted) return;

            if (failedInterviewsCount > 0)
            {
                this.ProgressTitle = string.Format(InterviewerUIResources.SendToSupervisor_SyncCompletedWithErrorsFormat, failedInterviewsCount);
                this.TransferingStatus = TransferingStatus.CompletedWithErrors;
            }
            else
            {
                this.ProgressTitle = InterviewerUIResources.SendToSupervisor_SyncCompleted;
                this.TransferingStatus = TransferingStatus.Completed;
            }
        }

        protected override string GetDeviceIdentification() => this.principal.CurrentUserIdentity.SupervisorId.FormatGuid();

        private async Task SynchronizeAsync()
        {
            lock (syncLockObject)
            {
                if (synchronizationCancellationTokenSource != null)
                {
                    return;
                }

                synchronizationCancellationTokenSource = new CancellationTokenSource();
                this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(synchronizationCancellationTokenSource.Token);
            }

            try
            {
                using (new CommunicationSession())
                {
                    this.synchronizationMode.Set(SynchronizationMode.Offline);
                    var synchronizationProcess = ServiceLocator.Current.GetInstance<ISynchronizationProcess>();

                    BlockingCollection<SyncProgressInfo> syncProgress = new BlockingCollection<SyncProgressInfo>();

                    var progress = new Progress<SyncProgressInfo>(o =>
                    {
                            if (o.Status == SynchronizationStatus.Canceled)
                                return;

                        if (o.TransferProgress == null)
                        {
                            this.ProgressStatus = o.Title;
                            this.ProgressStatusDescription = o.Description;
                            this.NetworkInfo = string.Empty;
                            this.ProgressInPercents = 0;
                        }
                        else
                        {
                            this.ProgressInPercents = o.TransferProgress?.Percent ?? 0;

                            if (o.TransferProgress.Speed.HasValue)
                            {
                                var receivedBytes = ByteSize.FromBytes(o.TransferProgress.Speed.Value);
                                var measurementInterval = TimeSpan.FromSeconds(1);
                                var transferingSpeed = receivedBytes.Per(measurementInterval).Humanize("#.##");

                                    this.NetworkInfo = string.Format(UIResources.OfflineSync_NetworkInfo,
                                        transferingSpeed,
                                    o.TransferProgress.Eta.Humanize(minUnit: TimeUnit.Second));
                            }
                        }

                        syncProgress.Add(o);

                        if (!o.IsRunning)
                        {
                            syncProgress.CompleteAdding();
                            this.OnComplete(o.Statistics.FailedInterviewsCount);
                        }
                    });

                    ReportProgressToSupervisor(syncProgress);

                    await synchronizationProcess.SynchronizeAsync(
                        progress,
                        synchronizationCancellationTokenSource.Token).ConfigureAwait(false);
                    this.synchronizationCompleteSource.NotifyOnCompletedSynchronization(true);
                }
            }
            finally
            {
                this.synchronizationMode.Set(SynchronizationMode.Online);
                synchronizationCancellationTokenSource = null;
            }
        }

        private void ReportProgressToSupervisor(BlockingCollection<SyncProgressInfo> syncProgress)
        {
#pragma warning disable 4014
            Task.Run(async () =>
            {
                while (!syncProgress.IsCompleted)
                {
                    SyncProgressInfo syncProgressInfo = null;
                    try
                    {
                        syncProgressInfo = syncProgress.Take();
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    if (syncProgressInfo != null)
                    {
                        try
                        {
                            var request = new SendSyncProgressInfoRequest
                            {
                                Info = syncProgressInfo,
                                InterviewerLogin = this.principal.CurrentUserIdentity.Name
                            };
                            await syncClient.SendAsync(request, this.cancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            syncProgress.CompleteAdding();
                        }
                    }
                }

            });
#pragma warning restore 4014
        }
    }
}
