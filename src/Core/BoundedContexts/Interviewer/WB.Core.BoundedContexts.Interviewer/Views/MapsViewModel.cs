using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class MapsViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ILogger logger;
        private readonly IMapService mapService;


        public MapSynchronizationViewModel Synchronization { set; get; }

        public MapsViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            MapSynchronizationViewModel synchronization,
            IMapService mapService,
            ILogger logger)
            : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.logger = logger;
            this.Synchronization = synchronization;
            this.Synchronization.SyncCompleted += this.Refresh;
            this.mapService = mapService;
        }

        public IMvxCommand SignOutCommand => new MvxCommand(this.SignOut);

        private void SignOut()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            this.viewModelNavigationService.SignOutAndNavigateToLogin();
        }

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () => await this.viewModelNavigationService.NavigateToDashboard());

        private MvxObservableCollection<MapItem> uiItems = new MvxObservableCollection<MapItem>();
        public MvxObservableCollection<MapItem> Maps
        {
            get => this.uiItems;
            protected set => this.RaiseAndSetIfChanged(ref this.uiItems, value);
        }

        private IMvxCommand mapSynchronizationCommand;
        public IMvxCommand MapSynchronizationCommand
        {
            get
            {
                return mapSynchronizationCommand ??
                       (mapSynchronizationCommand = new MvxCommand(this.RunMapSync,
                           () => !this.Synchronization.IsSynchronizationInProgress));
            }
        }

        public string MapsTitle => InterviewerUIResources.Maps_Title;

        
        private void RunMapSync()
        {
            if (this.viewModelNavigationService.HasPendingOperations)
            {
                this.viewModelNavigationService.ShowWaitMessage();
                return;
            }

            
            this.Synchronization.Synchronize();
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set
            {
                this.isInProgress = value;
                RaisePropertyChanged(); 
            }
        }

        private void Refresh(object sender, EventArgs e)
        {
            this.RefreshMaps();
        }

        private void RefreshMaps()
        {
            UpdateUiItems();
        }

        protected void UpdateUiItems() => Task.Run(() =>
        {
            this.IsInProgress = true;

            try
            {
                var newItems = this.mapService.GetAvailableMaps().Select(x => new MapItem()
                {
                    MapName = x.MapName
                }  ).ToList();
                this.Maps.ReplaceWith(newItems);
            }
            finally
            {
                this.IsInProgress = false;
            }
            
        });

        public IMvxCommand NavigateToDiagnosticsPageCommand =>
            new MvxCommand(() => this.viewModelNavigationService.NavigateTo<DiagnosticsViewModel>());

        public override void Load()
        {
            this.Synchronization.Init();
            RefreshMaps();
        }
    }

    public class MapSynchronizationViewModel : MvxNotifyPropertyChanged
    {

        public IMapSyncBackgroundService MapSyncBackgroundService { get; set; }
        public event EventHandler SyncCompleted;


        private bool synchronizationErrorOccured;
        public bool SynchronizationErrorOccured
        {
            get => this.synchronizationErrorOccured;
            set
            {
                this.synchronizationErrorOccured = value;
                this.RaisePropertyChanged();
            }
        }

        

        private bool isSynchronizationInfoShowed;
        public bool IsSynchronizationInfoShowed
        {
            get => this.isSynchronizationInfoShowed;
            set
            {
                this.isSynchronizationInfoShowed = value;
                this.RaisePropertyChanged();
            }
        }

        private string processOperation;
        public string ProcessOperation
        {
            get => this.processOperation;
            set
            {
                if (this.processOperation == value) return;

                this.processOperation = value;
                this.RaisePropertyChanged();
            }
        }

        private string processOperationDescription;
        public string ProcessOperationDescription
        {
            get => this.processOperationDescription;
            set
            {
                this.processOperationDescription = value;
                this.RaisePropertyChanged();
            }
        }

        private bool isSynchronizationInProgress;
        public bool IsSynchronizationInProgress
        {
            get => this.isSynchronizationInProgress;
            set
            {
                this.isSynchronizationInProgress = value;
                this.RaisePropertyChanged();
            }
        }
        private CancellationTokenSource synchronizationCancellationTokenSource;

        public IMvxCommand CancelSynchronizationCommand => new MvxCommand(this.CancelSynchronizaion);
        public IMvxCommand HideSynchronizationCommand => new MvxCommand(this.HideSynchronizaion);

        public void HideSynchronizaion()
        {
            this.IsSynchronizationInfoShowed = false;
        }

        private void CancelSynchronizaion()
        {
            if (this.synchronizationCancellationTokenSource != null && !this.synchronizationCancellationTokenSource.IsCancellationRequested)
                this.synchronizationCancellationTokenSource.Cancel();
        }

        public void Init()
        {
            var mapSyncProgressStatus = this.MapSyncBackgroundService?.CurrentProgress;
            if (mapSyncProgressStatus != null)
            {
                mapSyncProgressStatus.Progress.ProgressChanged += ProgressOnProgressChanged;
                this.synchronizationCancellationTokenSource = mapSyncProgressStatus.CancellationTokenSource;
            }
        }

        public void Synchronize()
        {
            this.IsSynchronizationInProgress = true;
            this.synchronizationCancellationTokenSource = new CancellationTokenSource();
            IsSynchronizationInfoShowed = true;

            MapSyncBackgroundService.SyncMaps();

            var mapSyncProgressStatus = this.MapSyncBackgroundService.CurrentProgress;
            if (mapSyncProgressStatus != null)
            {
                mapSyncProgressStatus.Progress.ProgressChanged += ProgressOnProgressChanged;
                this.synchronizationCancellationTokenSource = mapSyncProgressStatus.CancellationTokenSource;
            }
        }

        private void ProgressOnProgressChanged(object sender, MapSyncProgress syncProgressInfo)
        {
            this.InvokeOnMainThread(() =>
            {
                this.IsSynchronizationInProgress = syncProgressInfo.IsRunning;
                this.ProcessOperation = syncProgressInfo.Title;
                
                this.IsSynchronizationInProgress = syncProgressInfo.IsRunning;
                
                if (!syncProgressInfo.IsRunning)
                {
                    this.OnSyncCompleted();
                }
            });
        }

        protected virtual void OnSyncCompleted()
        {
            this.SyncCompleted?.Invoke(this, EventArgs.Empty);
        }
    }


    public class MapItem : MvxNotifyPropertyChanged
    {
        private string mapName;

        public string MapName
        {
            get => mapName;
            set => SetProperty(ref mapName, value);
        }
    }
}