﻿using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Shared.Extensions.Services;
using Xamarin.Essentials;
using Location = Esri.ArcGISRuntime.Location.Location;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace WB.UI.Shared.Extensions.ViewModels
{
    public abstract class BaseMapInteractionViewModel<TParam> : BaseViewModel<TParam>
    {
        protected const string ShapefileLayerName = "shapefile";

        protected readonly ILogger logger;
        private readonly IMapService mapService;
        protected readonly IUserInteractionService UserInteractionService;
        protected readonly IMvxNavigationService NavigationService;
        private readonly IEnumeratorSettings enumeratorSettings;
        private readonly IMapUtilityService mapUtilityService;
        protected readonly IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher;
        private readonly IPermissionsService permissionsService;
        private readonly IEnumeratorSettings settings;

        protected BaseMapInteractionViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IMapService mapService,
            IUserInteractionService userInteractionService,
            ILogger logger,
            IEnumeratorSettings enumeratorSettings,
            IMapUtilityService mapUtilityService,
            IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher,
            IPermissionsService permissionsService,
            IEnumeratorSettings settings) 
            : base(principal, viewModelNavigationService)
        {
            this.UserInteractionService = userInteractionService;
            this.mapService = mapService;
            this.logger = logger;
            this.NavigationService = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
            this.enumeratorSettings = enumeratorSettings;
            this.mapUtilityService = mapUtilityService;
            this.mainThreadAsyncDispatcher = mainThreadAsyncDispatcher;
            this.permissionsService = permissionsService;
            this.settings = settings;
        }

        public abstract Task OnMapLoaded();
        public abstract MapDescription GetSelectedMap(MvxObservableCollection<MapDescription> mapsToSelectFrom);

        protected string DefaultMapName = "";
        
        public override async Task Initialize()
        {
            await base.Initialize();

            try
            {
                this.AvailableShapefiles =
                    new MvxObservableCollection<ShapefileDescription>(this.mapService.GetAvailableShapefiles());

                var includeOnlineMaps = !string.IsNullOrEmpty(settings.EsriApiKey);
                ArcGISRuntimeEnvironment.ApiKey = includeOnlineMaps ? settings.EsriApiKey : String.Empty;
                
                var localMaps = this.mapService.GetAvailableMaps(includeOnlineMaps);
                var defaultMap = this.mapService.PrepareAndGetDefaultMapOrNull();
                if (defaultMap != null)
                {
                    localMaps.Add(defaultMap);
                    DefaultMapName = defaultMap.MapName;
                }
                
                if (localMaps.Count == 0)
                    return;
                
                this.AvailableMaps = new MvxObservableCollection<MapDescription>(localMaps);

                var defaultBaseMap = await mapUtilityService.GetBaseMap(defaultMap).ConfigureAwait(false);
                await defaultBaseMap.LoadAsync().ConfigureAwait(false);
                this.Map = new Map(defaultBaseMap);

                if (defaultBaseMap?.BaseLayers.Count > 0 && defaultBaseMap?.BaseLayers[0]?.FullExtent != null)
                    this.Map.MaxExtent = defaultBaseMap.BaseLayers[0].FullExtent;
                
                await this.Map.LoadAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Error("Error on map initialization", e);
                throw;
            }
        }

        public bool FirstLoad { get; set; }

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
        
        public IMvxAsyncCommand ShowFullMapCommand => new MvxAsyncCommand(async () =>
        {
            if (this.Map?.Basemap?.BaseLayers.Count > 0 && this.Map?.Basemap?.BaseLayers[0]?.FullExtent != null)
            {
                await MapView.SetViewpointGeometryAsync(this.Map.Basemap.BaseLayers[0].FullExtent);

                ShowedFullMap();
            }
        });

        protected virtual void ShowedFullMap() { }

        public IMvxAsyncCommand SwitchLocatorCommand => new MvxAsyncCommand(async () =>
        {
            if (!IsLocationServiceSwitchEnabled)
                return;

            //try to workaround Esri crash with location service
            //Esri case 02209395
            try
            {
                await this.permissionsService.AssureHasPermissionOrThrow<Permissions.LocationWhenInUse>()
                    .ConfigureAwait(false);

                IsLocationServiceSwitchEnabled = false;

                if (!this.MapView.LocationDisplay.IsEnabled)
                    this.MapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;
                
                var locationDisplayDataSource = this.MapView.LocationDisplay.DataSource;
                if (locationDisplayDataSource != null)
                {
                    //try to stop service first to avoid crash
                    await locationDisplayDataSource.StopAsync();
                    locationDisplayDataSource.StatusChanged += DataSourceOnStatusChanged;
                    await locationDisplayDataSource.StartAsync();
                }

                this.MapView.LocationDisplay.IsEnabled = true;
                this.MapView.LocationDisplay.LocationChanged += LocationDisplayOnLocationChanged;
            }
            catch (MissingPermissionsException mp) when (mp.PermissionType == typeof(Permissions.LocationWhenInUse))
            {
                this.UserInteractionService.ShowToast(UIResources.MissingPermissions_MapsLocation);
                return;
            }
            catch (Exception exc)
            {
                logger.Error("Error occurred on map location start.", exc);
            }
        });

        private void DataSourceOnStatusChanged(object sender, LocationDataSourceStatus e)
        {
            if(e == LocationDataSourceStatus.FailedToStart)
                this.UserInteractionService.ShowToast(UIResources.AreaMap_LocationDataSourceFailed);
        }

        protected async void LocationDisplayOnLocationChanged(object sender, Location e)
        {
            //show only once
            this.MapView.LocationDisplay.LocationChanged -= LocationDisplayOnLocationChanged;

            if (e?.Position == null) return;
            if (this.Map?.Basemap?.BaseLayers.Count <= 0) return;
            var extent = this.MapView.Map?.Basemap?.BaseLayers[0].FullExtent;

            if (extent == null) return;
            var point = GeometryEngine.Project(e.Position, extent.SpatialReference);

            
            if (!GeometryEngine.Contains(extent, point))
            {
                this.UserInteractionService.ShowToast(UIResources.AreaMap_LocationOutOfBoundaries);
            }

            if (ShapeFileLoaded)
            {
                var shapeLayer = this.Map?.OperationalLayers[0];
                var shapeExtent = shapeLayer?.FullExtent;
                var sPoint = GeometryEngine.Project(e.Position, shapeExtent.SpatialReference);
                if (GeometryEngine.Contains(shapeExtent, sPoint))
                {
                    var featureLayer = (FeatureLayer)shapeLayer;
                    var shapefileFeatureTable = (ShapefileFeatureTable)featureLayer?.FeatureTable;
                    var labelFieldIndex = shapefileFeatureTable.Fields.ToList().FindIndex(f => 
                        string.Compare(f.Name, "label", StringComparison.OrdinalIgnoreCase) == 0);
                    
                    if (labelFieldIndex < 0)
                        return;
                    var features = await shapefileFeatureTable.QueryFeaturesAsync(new QueryParameters()
                    {
                        Geometry = sPoint,
                        
                    }).ConfigureAwait(false);
                    var featuresField = features.Fields[labelFieldIndex];
                    var featuresFieldName = featuresField.Name;
                    this.UserInteractionService.ShowToast(featuresFieldName);
                }
            }
        }

        public override async void ViewCreated()
        {
            base.ViewCreated();

            // we use MapView and Map properties in MapControlCreatedAsync, need wait to init its
            if (InitializeTask?.Task != null)
                await InitializeTask.Task.ConfigureAwait(false);
            
            await this.MapControlCreatedAsync();
        }

        public async Task MapControlCreatedAsync()
        {
            if (this.Map != null && this.Map?.LoadStatus != LoadStatus.FailedToLoad)
            {
                this.FirstLoad = true;
                
                var mapToDisplay = GetSelectedMap(this.AvailableMaps);
                var selectedMapToLoad = mapToDisplay?.MapName;
                await UpdateBaseMap(selectedMapToLoad).ConfigureAwait(false);
                await OnMapLoaded().ConfigureAwait(false);
            }
        }

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
        
        public IMvxAsyncCommand ShowAllItemsCommand => new MvxAsyncCommand(async () =>
        {
            await SetViewToValues();
        });

        protected abstract Task SetViewToValues();

        protected async Task UpdateBaseMap(string selectedMapToLoad)
        {
            logger.Debug($"UpdateBaseMap was called with map: {selectedMapToLoad}" );
            
            var existingMap = this.AvailableMaps.FirstOrDefault(x => x.MapName == selectedMapToLoad);
            if (existingMap == null) return;

            if (this.SelectedMap != selectedMapToLoad)
            {
                var baseMap = await mapUtilityService.GetBaseMap(existingMap);
                if (baseMap == null) return;
                
                this.SelectedMap = selectedMapToLoad;
                this.Map.Basemap = baseMap;
            }

            if (this.Map.LoadStatus == LoadStatus.Loaded 
                && this.Map.Basemap?.BaseLayers.Count > 0 
                && this.Map.Basemap?.BaseLayers[0]?.FullExtent != null)
            {
                if (FirstLoad)
                {
                    FirstLoad = false;
                    await MapView.SetViewpointGeometryAsync(this.Map.Basemap.BaseLayers[0].FullExtent);
                }
                
                if (this.MapView?.VisibleArea != null)
                {
                    logger.Debug("projecting areas" );
                    await mainThreadAsyncDispatcher.ExecuteOnMainThreadAsync(() =>
                    {
                        var projectedArea =
                            GeometryEngine.Project(this.MapView.VisibleArea, this.Map.Basemap.BaseLayers[0].SpatialReference);

                        if (projectedArea != null &&
                            !GeometryEngine.Intersects(this.Map.Basemap.BaseLayers[0].FullExtent, projectedArea))
                            this.UserInteractionService.ShowToast(UIResources
                                .AreaMap_MapIsOutOfVisibleBoundaries);
                    });
                }
            }
            
            if (LastMap != existingMap.MapName)
                LastMap = existingMap.MapName;
        }

        private Map map;
        public Map Map
        {
            get => this.map;
            set => this.RaiseAndSetIfChanged(ref this.map, value);
        }

        private bool isDisposed;
        private bool shapeFileLoaded;
        
        protected ShapefileFeatureTable LoadedShapefile;

        public MapView MapView { get; set; }

        public IMvxAsyncCommand RotateMapToNorth => new MvxAsyncCommand(async () =>
        {
            if (this.MapView != null && this.MapView.MapScale != Double.NaN)
                await this.MapView.SetViewpointRotationAsync(0);
        });

        public IMvxAsyncCommand ZoomMapIn => new MvxAsyncCommand(async () =>
        {
            if (this.MapView != null && this.MapView.MapScale != Double.NaN)
                await this.MapView.SetViewpointScaleAsync(this.MapView.MapScale / 1.3);
        });

        public IMvxAsyncCommand ZoomMapOut => new MvxAsyncCommand(async () =>
        {
            if (this.MapView != null && this.MapView.MapScale != Double.NaN)
                await this.MapView.SetViewpointScaleAsync(this.MapView.MapScale * 1.3);
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
                fullPathToShapefile = await UserInteractionService.SelectOneOptionFromList(UIResources.AreaMap_SelectShapefile, options);
                if (string.IsNullOrEmpty(fullPathToShapefile))
                    return;
            }

            try 
            {
                LoadedShapefile = await ShapefileFeatureTable.OpenAsync(fullPathToShapefile);
                
                var newFeatureLayer = await mapUtilityService.GetShapefileAsFeatureLayer(LoadedShapefile);
                newFeatureLayer.Name = ShapefileLayerName;
                
                RemoveShapefileLayer();

                // Add the feature layer to the map
                this.MapView.Map.OperationalLayers.Add(newFeatureLayer);

                // Zoom the map to the extent of the shapefile
                if(newFeatureLayer.FullExtent != null)
                    await this.MapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent);

                ShapeFileLoaded = true;
                await AfterShapefileLoadedHandler();
            }
            catch (Exception e)
            {
                LoadedShapefile = null;
                logger.Error("Error on shapefile loading", e);
                UserInteractionService.ShowToast(UIResources.AreaMap_ErrorOnShapefileLoading);
            }
        });

        protected virtual Task AfterShapefileLoadedHandler()
        {
            return Task.CompletedTask;
        }

        private void RemoveShapefileLayer()
        {
            var existedLayer = this.MapView.Map?.OperationalLayers.FirstOrDefault(l => l.Name == ShapefileLayerName);
            if (existedLayer != null)
                this.MapView.Map.OperationalLayers.Remove(existedLayer);
        }

        public bool ShapeFileLoaded
        {
            get => shapeFileLoaded;
            set => this.RaiseAndSetIfChanged(ref shapeFileLoaded, value);
        }

        private bool isWarningVisible;
        public bool IsWarningVisible
        {
            get => isWarningVisible;
            set => this.RaiseAndSetIfChanged(ref isWarningVisible, value);
        }

        private string warning;
        public string Warning
        {
            get => this.warning;
            set => this.RaiseAndSetIfChanged(ref this.warning, value);
        }
        
        public IMvxCommand HideShapefile => new MvxAsyncCommand(async() =>
        {
            if (!ShapeFileLoaded)
                return;

            try
            {
                RemoveShapefileLayer();
                ShapeFileLoaded = false;
                await AfterShapefileLoadedHandler();
            }
            catch (Exception e)
            {
                logger.Error("Error on shapefile handling", e);
                UserInteractionService.ShowToast(UIResources.AreaMap_ErrorOnShapefileLoading);
            }
        });

        public override void Dispose()
        {
            if (isDisposed)
                return;
            
            isDisposed = true;
            
            base.Dispose();
            
            if (this.MapView?.LocationDisplay != null)
                this.MapView.LocationDisplay.LocationChanged -= LocationDisplayOnLocationChanged;
            
            if (this.MapView?.LocationDisplay?.DataSource != null)
                this.MapView.LocationDisplay.DataSource.StatusChanged -= DataSourceOnStatusChanged;
            
           
            this.MapView?.Dispose();
        }
    }
}
