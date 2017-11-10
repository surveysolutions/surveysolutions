using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
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
                    MapName = x.MapName,
                    CreationDate = x.CreationDate,
                    Size = x.Size
                }  
                ).ToList();
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


    public class MapItem : MvxNotifyPropertyChanged
    {
        private string mapName;

        public string MapName
        {
            get => mapName;
            set => SetProperty(ref mapName, value);
        }

        private long size;
        public long Size {
            get => size;
            set => SetProperty(ref size, value);
        }

        private DateTime creationDate;
        public DateTime CreationDate
        {
            get => creationDate;
            set => SetProperty(ref creationDate, value);
        }
    }
}