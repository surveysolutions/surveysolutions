using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using MvvmCross;
using MvvmCross.Commands;
using Plugin.Permissions.Abstractions;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;
using InterviewerUIResources = WB.Core.BoundedContexts.Interviewer.Properties.InterviewerUIResources;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    [ExcludeFromCodeCoverage()] // TODO: remove attribute when UI binding completed
    public class OfflineInterviewerSyncViewModel : BaseOfflineSyncViewModel
    {
        private readonly IInterviewerPrincipal principal;
        private readonly ISynchronizationMode synchronizationMode;
        private readonly ISynchronizationCompleteSource synchronizationCompleteSource;
        private CancellationTokenSource synchronizationCancellationTokenSource;

        public OfflineInterviewerSyncViewModel(IInterviewerPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IPermissionsService permissions,
            IEnumeratorSettings settings,
            ISynchronizationMode synchronizationMode,
            INearbyConnection nearbyConnection,
            ISynchronizationCompleteSource synchronizationCompleteSource)
            : base(principal, viewModelNavigationService, permissions, nearbyConnection, settings)
        {
            this.principal = principal;
            this.synchronizationMode = synchronizationMode;
            this.synchronizationCompleteSource = synchronizationCompleteSource;
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

        private string progressDescription;
        public string ProgressDescription
        {
            get => this.progressDescription;
            set => this.SetProperty(ref this.progressDescription, value);
        }

        private TransferingStatus transferingStatus;
        public TransferingStatus TransferingStatus
        {
            get => this.transferingStatus;
            set => this.SetProperty(ref this.transferingStatus, value);
        }

        public override Task Initialize()
        {
            this.ReInitialize();

            return base.Initialize();
        }

        private void ReInitialize()
        {
            this.TransferingStatus = TransferingStatus.WaitingDevice;
            this.Title = InterviewerUIResources.SendToSupervisor_MovingToSupervisorDevice;
            this.ProgressTitle = InterviewerUIResources.SendToSupervisor_CheckSupervisorDevice;
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
                this.synchronizationCancellationTokenSource.Cancel();

            this.ProgressTitle = InterviewerUIResources.SendToSupervisor_TransferWasAborted;
            this.TransferingStatus = TransferingStatus.Aborted;
        }

        private async Task Retry()
        {
            this.ReInitialize();
            await OnGoogleApiReady();
        }

        protected override async Task OnGoogleApiReady()
        {
            this.StopDiscovery();
            await StartDiscovery();
        }

        private async Task StartDiscovery()
        {
            await this.permissions.AssureHasPermission(Permission.Location);

            SetStatus(ConnectionStatus.StartDiscovering, $"Starting discovery");
            var serviceName = this.GetServiceName();
            var result = await this.nearbyConnection.StartDiscovery(serviceName);

            if (!SetErrorOnResultIfNeeded(result)) return;
        }

        private void StopDiscovery()
        {
            //this.nearbyConnection.StopAllEndpoint();
            this.nearbyConnection.StopDiscovery();
        }

        protected override async void Connected(string connectedEndpoint, string connectedTo)
        {
            this.Title = string.Format(InterviewerUIResources.SendToSupervisor_MovingToSupervisorFormat, connectedTo);
            this.ProgressTitle = InterviewerUIResources.SendToSupervisor_TransferInProgress;
            this.TransferingStatus = TransferingStatus.Transferring;

            await this.SynchronizeAsync();
        }

        protected override void Disconnected(string endpoint)
        {
            if (new[] {TransferingStatus.Completed, TransferingStatus.Aborted}.Contains(this.TransferingStatus)) return;

            this.OnTerminateTransferring();
        }

        private void OnTerminateTransferring()
        {
            this.ProgressTitle = InterviewerUIResources.SendToSupervisor_SupervisorTerminateTransfering;
            this.TransferingStatus = TransferingStatus.CompletedWithErrors;
        }

        protected override void OnError(string errorMessage, ConnectionStatusCode errorCode)
        {
            this.ProgressTitle = InterviewerUIResources.SendToSupervisor_SupervisorNotFound;
            this.TransferingStatus = TransferingStatus.Failed;
        }

        private void OnComplete()
        {
            this.ProgressTitle = InterviewerUIResources.SendToSupervisor_SyncCompleted;
            this.TransferingStatus = TransferingStatus.Completed;
            this.StopDiscovery();
        }

        protected override string GetDeviceIdentification() => this.principal.CurrentUserIdentity.SupervisorId.FormatGuid();

        private async Task SynchronizeAsync()
        {
            if (this.synchronizationCancellationTokenSource != null)
            {
                return;
            }

            this.synchronizationCancellationTokenSource = new CancellationTokenSource();

            try
            {
                using (new CommunicationSession())
                {
                    this.synchronizationMode.Set(SynchronizationMode.Offline);
                    var synchronizationProcess = Mvx.Resolve<ISynchronizationProcess>();

                    await synchronizationProcess.SynchronizeAsync(
                        new Progress<SyncProgressInfo>(o =>
                        {
                            if (o.TransferProgress == null)
                            {
                                this.ProgressStatus = o.Title;
                                this.ProgressStatusDescription = o.Description;
                                this.ProgressDescription = string.Empty;
                            }
                            else
                            {
                                this.ProgressInPercents = o.TransferProgress?.Percent ?? 0;
                                string speed = null;
                                if (o.TransferProgress.Speed.HasValue)
                                {
                                    speed = o.TransferProgress.Speed.Value.Bytes().ToString("0.00") + "/s. ";
                                }

                                this.ProgressDescription = $"{speed ?? ""} in {o.TransferProgress.Eta.Humanize()}";
                            }

                            if(o.HasErrors) this.OnTerminateTransferring();
                            if(!o.IsRunning && !o.HasErrors) this.OnComplete();
                        }),
                        this.synchronizationCancellationTokenSource.Token);

                    this.synchronizationCompleteSource.NotifyOnCompletedSynchronization(true);
                }
            }
            finally
            {
                this.synchronizationMode.Set(SynchronizationMode.Online);
                synchronizationCancellationTokenSource = null;
            }
        }
    }
}
