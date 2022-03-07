using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Shared.Extensions.Services;

namespace WB.UI.Shared.Extensions.ViewModels
{
    public abstract class BaseMapInteractionViewModel<TParam> : BaseViewModel<TParam>, IDisposable
    {
        readonly ILogger logger;
        private readonly IMapService mapService;
        protected readonly IUserInteractionService userInteractionService;

        private readonly IFileSystemAccessor fileSystemAccessor;
        protected readonly IMvxNavigationService navigationService;
        private readonly IEnumeratorSettings enumeratorSettings;
        private readonly IMapUtilityService mapUtilityService;

        protected BaseMapInteractionViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IMapService mapService,
            IUserInteractionService userInteractionService,
            ILogger logger,
            IFileSystemAccessor fileSystemAccessor,
            IEnumeratorSettings enumeratorSettings,
            IMapUtilityService mapUtilityService) 
            : base(principal, viewModelNavigationService)
        {
            this.userInteractionService = userInteractionService;
            this.mapService = mapService;
            this.logger = logger;
            this.fileSystemAccessor = fileSystemAccessor;
            this.navigationService = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
            this.enumeratorSettings = enumeratorSettings;
            this.mapUtilityService = mapUtilityService;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            try
            {
                this.AvailableShapefiles =
                    new MvxObservableCollection<ShapefileDescription>(this.mapService.GetAvailableShapefiles());
                
                var localMaps = this.mapService.GetAvailableMaps(true);
                var defaultMap = this.mapService.PrepareAndGetDefaultMapOrNull();
                if (defaultMap != null)
                {
                    localMaps.Add(defaultMap);
                }
                
                if (localMaps.Count == 0)
                    return;
                
                this.AvailableMaps = new MvxObservableCollection<MapDescription>(localMaps);
                var mapToDisplay = GetSelectedMap(this.AvailableMaps);
                
                var defaultBaseMap = await mapUtilityService.GetBaseMap(mapToDisplay).ConfigureAwait(false);
                //var basemap = await MapUtilityService.GetBaseMap(this.fileSystemAccessor, mapToDisplay).ConfigureAwait(false);
                this.Map = new Map(defaultBaseMap);

                this.SelectedMap = mapToDisplay.MapName;

            }
            catch (Exception e)
            {
                logger.Error("Error on map initialization", e);
                throw;
            }
            
            this.Map.Loaded += MapOnLoaded;
        }
        
        protected string LastMap
        {
            set => enumeratorSettings.SetLastOpenedMapName(value);
            get => enumeratorSettings.LastOpenedMapName;
        }

        private bool isLocationServiceSwitchEnabled = true;
        public bool IsLocationServiceSwitchEnabled
        {
            get => this.isLocationServiceSwitchEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isLocationServiceSwitchEnabled, value);
        }
        
        public IMvxAsyncCommand SwitchLocatorCommand => new MvxAsyncCommand(async () =>
        {
            if (IsLocationServiceSwitchEnabled)
                return;

            //try to workaround Esri crash with location service
            //Esri case 02209395
            try
            {
                IsLocationServiceSwitchEnabled = true;

                if (!this.MapView.LocationDisplay.IsEnabled)
                    this.MapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;

                //try to stop service first to avoid crash
                await this.MapView.LocationDisplay.DataSource.StopAsync();

                this.MapView.LocationDisplay.DataSource.StatusChanged += DataSourceOnStatusChanged; 

                await this.MapView.LocationDisplay.DataSource.StartAsync();
                this.MapView.LocationDisplay.IsEnabled = true;
                this.MapView.LocationDisplay.LocationChanged += LocationDisplayOnLocationChanged;
            }
            catch (Exception exc)
            {
                logger.Error("Error occurred on map location start.", exc);
            }
        });

        private void DataSourceOnStatusChanged(object sender, LocationDataSourceStatus e)
        {
            if(e == LocationDataSourceStatus.FailedToStart)
                this.userInteractionService.ShowToast(UIResources.AreaMap_LocationDataSourceFailed);
        }

        protected void LocationDisplayOnLocationChanged(object sender, Location e)
        {
            //show only once
            this.MapView.LocationDisplay.LocationChanged -= LocationDisplayOnLocationChanged;

            if (e.Position == null) { return; }

            if (this.Map?.Basemap?.BaseLayers.Count <= 0) return;
            
            var extent = this.MapView.Map.Basemap.BaseLayers[0].FullExtent;

            var point = GeometryEngine.Project(e.Position, extent.SpatialReference);

            if (!GeometryEngine.Contains(extent, point))
            {
                this.userInteractionService.ShowToast(UIResources.AreaMap_LocationOutOfBoundaries);
            }
        }

        public abstract Task OnMapLoaded();


        public async void MapOnLoaded(object sender, EventArgs e)
        {
            await UpdateBaseMap().ConfigureAwait(false);
            await OnMapLoaded().ConfigureAwait(false);

            if (AvailableShapefiles.Count == 1)
                await LoadShapefile.ExecuteAsync();
        }

        public abstract MapDescription GetSelectedMap(MvxObservableCollection<MapDescription> mapsToSelectFrom);

        private MvxObservableCollection<MapDescription> availableMaps = new MvxObservableCollection<MapDescription>();
        public MvxObservableCollection<MapDescription> AvailableMaps
        {
            get => this.availableMaps;
            protected set => this.RaiseAndSetIfChanged(ref this.availableMaps, value);
        }

        private MvxObservableCollection<ShapefileDescription> availableShapefiles = new MvxObservableCollection<ShapefileDescription>();
        public MvxObservableCollection<ShapefileDescription> AvailableShapefiles
        {
            get => this.availableShapefiles;
            protected set => this.RaiseAndSetIfChanged(ref this.availableShapefiles, value);
        }

        private string selectedMap;
        public string SelectedMap
        {
            get => this.selectedMap;
            set => this.RaiseAndSetIfChanged(ref this.selectedMap, value);
        }

        public async Task UpdateBaseMap()
        {
            var existingMap = this.AvailableMaps.FirstOrDefault(x => x.MapName == this.SelectedMap);

            if (existingMap != null)
            {
                var baseMap = await mapUtilityService.GetBaseMap(existingMap);
                if (baseMap != null)
                {
                    this.Map.Basemap = baseMap;

                    if (baseMap?.BaseLayers.Count > 0 && baseMap?.BaseLayers[0]?.FullExtent != null && this.MapView?.VisibleArea != null)
                    {
                        var projectedArea = GeometryEngine.Project(this.MapView.VisibleArea,
                            baseMap.BaseLayers[0].SpatialReference);

                        if (projectedArea!= null && !GeometryEngine.Intersects(baseMap.BaseLayers[0].FullExtent, projectedArea))
                            this.userInteractionService.ShowToast(UIResources.AreaMap_MapIsOutOfVisibleBoundaries);
                    }

                    /*if (basemap?.BaseLayers[0]?.FullExtent != null)
                        await MapView.SetViewpointGeometryAsync(basemap.BaseLayers[0].FullExtent);*/
                    
                    LastMap = this.SelectedMap;
                }
            }
        }


        private Map map;
        public Map Map
        {
            get => this.map;
            set => this.RaiseAndSetIfChanged(ref this.map, value);
        }

        private MapView mapView;
        private bool isDisposed;
        private bool showedBoundaries;

        public MapView MapView
        {
            get { return mapView ??= new MapView(Application.Context); }
            set => mapView = value;
        }

        public IMvxAsyncCommand RotateMapToNorth => new MvxAsyncCommand(async () =>
            await this.MapView?.SetViewpointRotationAsync(0));

        public IMvxAsyncCommand ZoomMapIn => new MvxAsyncCommand(async () =>
            await this.MapView?.SetViewpointScaleAsync(this.MapView.MapScale / 1.3));

        public IMvxAsyncCommand ZoomMapOut => new MvxAsyncCommand(async () =>
            await this.MapView?.SetViewpointScaleAsync(this.MapView.MapScale * 1.3));

        public IMvxCommand SaveAreaCommand => new MvxCommand(() =>
        {
            var command = this.MapView.SketchEditor.CompleteCommand;
            if (this.MapView.SketchEditor.CompleteCommand.CanExecute(command))
            {
                this.MapView.SketchEditor.CompleteCommand.Execute(command);
            }
            else
            {
                this.userInteractionService.ShowToast(UIResources.AreaMap_NoChangesInfo);
            }
        });

        public IMvxAsyncCommand LoadShapefile => new MvxAsyncCommand(async () =>
        {
            if (AvailableShapefiles.Count < 1)
                return;

            var fullPathToShapefile = AvailableShapefiles.First().FullPath;
            if (AvailableShapefiles.Count > 1)
            {
                var options = AvailableShapefiles.Select(s =>
                    new Tuple<string, string>(s.ShapefileName,
                        s.FullPath)
                ).ToArray();
                fullPathToShapefile = await userInteractionService.SelectOneOptionFromList(UIResources.AreaMap_SelectShapefile, options);
                if (string.IsNullOrEmpty(fullPathToShapefile))
                    return;
            }

            try
            {
                var newFeatureLayer = await mapUtilityService.GetShapefileAsFeatureLayer(fullPathToShapefile);
                
                this.MapView.Map.OperationalLayers.Clear();

                // Add the feature layer to the map
                this.MapView.Map.OperationalLayers.Add(newFeatureLayer);

                // Zoom the map to the extent of the shapefile
                await this.MapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent);

                ShowedBoundaries = true;
            }
            catch (Exception e)
            {
                logger.Error("Error on shapefile loading", e);
                userInteractionService.ShowToast(UIResources.AreaMap_ErrorOnShapefileLoading);
            }
        });

        public bool ShowedBoundaries
        {
            get => showedBoundaries;
            set => this.RaiseAndSetIfChanged(ref showedBoundaries, value);
        }

        public IMvxCommand HideShapefile => new MvxCommand(() =>
        {
            if (!ShowedBoundaries)
                return;

            try
            {
                this.MapView.Map.OperationalLayers.Clear();
                ShowedBoundaries = false;
            }
            catch (Exception e)
            {
                logger.Error("Error on shapefile loading", e);
                userInteractionService.ShowToast(UIResources.AreaMap_ErrorOnShapefileLoading);
            }
        });

        public override void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            
            base.Dispose();
            
            if(this.MapView?.LocationDisplay!= null )
                this.MapView.LocationDisplay.LocationChanged -= LocationDisplayOnLocationChanged;
            
            if (this.MapView?.LocationDisplay?.DataSource != null)
                this.MapView.LocationDisplay.DataSource.StatusChanged -= DataSourceOnStatusChanged;
            
            if(this.Map != null)
                this.Map.Loaded -= MapOnLoaded;
        }
    }
}
