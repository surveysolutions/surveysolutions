using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using Xamarin.Essentials;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class MapsViewModel : BaseViewModel
    {
        private readonly IMapService mapService;
        private readonly IPermissionsService permissions;
        private readonly IUserInteractionService userInteractionService;

        public MapSynchronizationViewModel Synchronization { set; get; }

        public MapsViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            MapSynchronizationViewModel synchronization,
            IMapService mapService,
            IPermissionsService permissions,
            IUserInteractionService userInteractionService,
            IMapInteractionService mapInteractionService)
            : base(principal, viewModelNavigationService)
        {
            this.Synchronization = synchronization;
            this.Synchronization.SyncCompleted += this.Refresh;
            this.mapService = mapService;
            this.permissions = permissions;
            this.userInteractionService = userInteractionService;
            this.mapInteractionService = mapInteractionService;

            this.Synchronization.Init();
        }

        public IMvxCommand SignOutCommand => new MvxAsyncCommand(this.SignOutAsync);

        private async Task SignOutAsync()
        {
            this.Synchronization.CancelSynchronizationCommand.Execute();
            await this.ViewModelNavigationService.SignOutAndNavigateToLoginAsync();
        }

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () => await this.ViewModelNavigationService.NavigateToDashboardAsync());

        private MvxObservableCollection<MapItem> uiItems = new MvxObservableCollection<MapItem>();
        public MvxObservableCollection<MapItem> Maps
        {
            get => this.uiItems;
            protected set => this.RaiseAndSetIfChanged(ref this.uiItems, value);
        }

        private IMvxAsyncCommand mapSynchronizationCommand;
        public IMvxAsyncCommand MapSynchronizationCommand
        {
            get
            {
                return mapSynchronizationCommand ??
                       (mapSynchronizationCommand = new MvxAsyncCommand(this.RunMapSyncAsync,
                           () => !this.Synchronization.IsSynchronizationInProgress));
            }
        }

        public string MapsTitle => EnumeratorUIResources.Maps_Title;

        
        private async Task RunMapSyncAsync()
        {
            if (this.ViewModelNavigationService.HasPendingOperations)
            {
                this.ViewModelNavigationService.ShowWaitMessage();
                return;
            }

            try
            {
                await this.permissions.AssureHasPermissionOrThrow<Permissions.StorageWrite>().ConfigureAwait(false);
            }
            catch (MissingPermissionsException)
            {
                this.userInteractionService.ShowToast(UIResources.MissingPermissions_Storage);
                return;
            }

            this.Synchronization.Synchronize();
        }

        private bool isInProgress;
        private IMapInteractionService mapInteractionService;

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
            UpdateUiItems();
        }

        
        protected void UpdateUiItems() => Task.Run(() =>
        {
            this.IsInProgress = true;

            try
            {
                var newItems = this.mapService.GetAvailableMaps().Select(x => new MapItem()
                {
                    MapName = x.MapFileName,
                    CreationDate = x.CreationDate,
                    Size = x.Size
                }).Concat(this.mapService.GetAvailableShapefiles().Select(x => new MapItem()
                {
                    MapName = x.ShapefileFileName,
                    CreationDate = x.CreationDate,
                    Size = x.Size
                }
                )).ToList();
                this.Maps = new MvxObservableCollection<MapItem>(newItems);
            }
            finally
            {
                this.IsInProgress = false;
            }
            
        });

        public IMvxCommand NavigateToDiagnosticsPageCommand =>
            new MvxAsyncCommand(this.ViewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>);

        public override async Task Initialize()
        {
            await base.Initialize();
           
            UpdateUiItems();
        }
        

    }
}
