using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using System.Drawing;
using Esri.ArcGISRuntime.Data;
using WB.Core.BoundedContexts.Interviewer.Views.CreateInterview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using MapView = Esri.ArcGISRuntime.UI.Controls.MapView;

namespace WB.UI.Shared.Extensions.CustomServices.MapDashboard
{
    public class MapDashboardViewModel: BaseViewModel
    {
        readonly ILogger logger;
        private readonly IUserInteractionService userInteractionService;

        private readonly IAssignmentDocumentsStorage assignmentsRepository;

        public MapDashboardViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IUserInteractionService userInteractionService,
            IMapService mapService,
            IFileSystemAccessor fileSystemAccessor,
            IAssignmentDocumentsStorage assignmentsRepository,
            ILogger logger) : base(principal, viewModelNavigationService)
        {
            this.logger = logger;
            this.userInteractionService = userInteractionService;
            this.mapService = mapService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.assignmentsRepository = assignmentsRepository;
        }

        private Map map;
        public Map Map
        {
            get => this.map;
            set => this.RaiseAndSetIfChanged(ref this.map, value);
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set => this.RaiseAndSetIfChanged(ref this.isInProgress, value);
        }

        public MapView MapView { get; set; }

        public override async Task Initialize()
        {
            await base.Initialize();

            var localMaps = this.mapService.GetAvailableMaps(true);
            var defaultMap = this.mapService.PrepareAndGetDefaultMap();
            localMaps.Add(defaultMap);

            this.AvailableMaps = new MvxObservableCollection<MapDescription>(localMaps);

            this.MapsList = this.AvailableMaps.Select(x => x.MapName).ToList();
            this.SelectedMap = defaultMap.MapName;

            Basemap baseMap = await GetBaseMap(defaultMap).ConfigureAwait(false);
            this.Map = new Map(baseMap);

            this.Map.Loaded += async delegate (object sender, EventArgs e)
            {
                await UpdateBaseMap().ConfigureAwait(false);
                await RefereshMarkersAsync();
            };
        }

        private GraphicsOverlay graphicsOverlay = new GraphicsOverlay();

        public IMvxCommand RefreshMarkersCommand =>
            new MvxAsyncCommand(async () => await RefereshMarkersAsync());

        private async Task RefereshMarkersAsync()
        {
            MapView.GraphicsOverlays.Add(graphicsOverlay);

            string questionnaireId = null;

            var assignments = this.assignmentsRepository
                //.Query(x=>x.QuestionnaireId == questionnaireId)
                .LoadAll()
                .Where(x => x.LocationLatitude != null).ToList();

            graphicsOverlay.Graphics.Clear();

            var markers = new List<Graphic>();
            foreach (var assignment in assignments)
            {
                var questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);
                var title = string.Format(EnumeratorUIResources.DashboardItem_Title, assignment.Title, questionnaireIdentity.Version);

                var interviewsByAssignmentCount = assignment.CreatedInterviewsCount ?? 0;
                var interviewsLeftByAssignmentCount = assignment.Quantity.GetValueOrDefault() - interviewsByAssignmentCount;

                string subTitle = "";

                if (assignment.Quantity.HasValue)
                {
                    if (interviewsLeftByAssignmentCount == 1)
                    {
                        subTitle = EnumeratorUIResources.Dashboard_AssignmentCard_SubTitleSingleInterivew;
                    }
                    else
                    {
                        subTitle = string.Format(EnumeratorUIResources.Dashboard_AssignmentCard_SubTitleCountdownFormat,
                            interviewsLeftByAssignmentCount, assignment.Quantity);
                    }
                }
                else
                {
                    subTitle = string.Format(EnumeratorUIResources.Dashboard_AssignmentCard_SubTitleCountdown_UnlimitedFormat,
                        assignment.Quantity.GetValueOrDefault());
                }

                bool canCreateInterview = !assignment.Quantity.HasValue || Math.Max(val1: 0, val2: interviewsLeftByAssignmentCount) > 0;

                MapPoint point = 
                    (MapPoint)GeometryEngine.Project(
                        new MapPoint(
                            assignment.LocationLongitude.Value,
                            assignment.LocationLatitude.Value,
                            SpatialReferences.Wgs84),
                        Map.SpatialReference);
                
                Graphic pointGraphic = new Graphic(
                    point, 
                    new []
                    {
                        new KeyValuePair<string, object>("id", assignment.Id),
                        new KeyValuePair<string, object>("title", title),
                        new KeyValuePair<string, object>("sub_title", subTitle),
                        new KeyValuePair<string, object>("can_create", canCreateInterview)

                    }, 
                    new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.Red, 20));
                
                markers.Add(pointGraphic);
            }

            if (markers.Count > 0)
            {
                graphicsOverlay.Graphics.AddRange(markers);
                MapView.GeoViewTapped += OnMapViewTapped;
            }

        }

        private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            double tolerance = 10d; // Use larger tolerance for touch
            int maximumResults = 1; // Only return one graphic  
            bool onlyReturnPopups = false; // Don't only return popups

            try
            {
                // Use the following method to identify graphics in a specific graphics overlay
                IdentifyGraphicsOverlayResult identifyResults = await MapView.IdentifyGraphicsOverlayAsync(
                    graphicsOverlay,
                    e.Position,
                    tolerance,
                    onlyReturnPopups,
                    maximumResults);

                // Check if we got results
                if (identifyResults.Graphics.Count > 0)
                {
                    if (identifyResults.Graphics[0].Geometry is MapPoint projectedLocation)
                    {
                        string id = identifyResults.Graphics[0].Attributes["id"].ToString();
                        string title = identifyResults.Graphics[0].Attributes["title"] as string;
                        string subTitle = identifyResults.Graphics[0].Attributes["sub_title"] as string;
                        bool canCreate = (bool)identifyResults.Graphics[0].Attributes["can_create"];

                        CalloutDefinition myCalloutDefinition = new CalloutDefinition("#" + id, $"{title}\r\n{subTitle}");
                        if (canCreate)
                        {
                            myCalloutDefinition.ButtonImage =
                                await new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Color.Blue, 25)
                                    .CreateSwatchAsync(30, 30, 96, System.Drawing.Color.White);
                            myCalloutDefinition.OnButtonClick += OnButtonClick;
                            myCalloutDefinition.Tag = id;
                        }

                        MapView.ShowCalloutAt(projectedLocation, myCalloutDefinition);
                    }
                }
                else
                {
                    MapView.DismissCallout();
                }
            }
            catch (Exception ex)
            {
                this.logger.Error("Error on ", ex);
            }
        }

        private void OnButtonClick(object calloutTag)
        {
            if(calloutTag != null && (Int32.TryParse(calloutTag as string, out int assignmentId)))
            {


                viewModelNavigationService
                    .NavigateToAsync<CreateAndLoadInterviewViewModel, CreateInterviewViewModelArg>(
                        new CreateInterviewViewModelArg()
                        {
                            AssignmentId = assignmentId,
                            InterviewId = Guid.NewGuid()
                        });
            }
        }

        public IMvxAsyncCommand<MapDescription> SwitchMapCommand => new MvxAsyncCommand<MapDescription>(async (mapDescription) =>
        {
            this.SelectedMap = mapDescription.MapName;
            IsPanelVisible = false;

            var geometry = this.MapView.SketchEditor.Geometry;
            await this.UpdateBaseMap();

            //update internal structures
            //SpatialReferense of new map could differ from initial
            if (geometry != null)
            {
                if (this.MapView != null && geometry != null && !this.MapView.SpatialReference.IsEqual(geometry.SpatialReference))
                    geometry = GeometryEngine.Project(geometry, this.MapView.SpatialReference);

                this.MapView?.SketchEditor.ClearGeometry();
                this.MapView?.SketchEditor.ReplaceGeometry(geometry);
            }
        });

        public IMvxCommand SwitchPanelCommand => new MvxCommand(() =>
        {
            IsPanelVisible = !IsPanelVisible;
        });

        private bool isPanelVisible;
        public bool IsPanelVisible
        {
            get => this.isPanelVisible;
            set => this.RaiseAndSetIfChanged(ref this.isPanelVisible, value);
        }

        public async Task<Basemap> GetBaseMap(MapDescription existingMap)
        {
            if (existingMap == null) return null;

            switch (existingMap.MapType)
            {
                case MapType.OnlineImagery:
                    return Basemap.CreateImagery();
                case MapType.OnlineImageryWithLabels:
                    return Basemap.CreateImageryWithLabels();
                case MapType.OnlineOpenStreetMap:
                    return Basemap.CreateOpenStreetMap();
                case MapType.LocalFile:
                    return await GetLocalMap(existingMap);
                default:
                    return null;
            }
        }

        private async Task<Basemap> GetLocalMap(MapDescription existingMap)
        {
            var mapFileExtention = this.fileSystemAccessor.GetFileExtension(existingMap.MapFullPath);

            switch (mapFileExtention)
            {
                case ".mmpk":
                    {
                        MobileMapPackage package = await MobileMapPackage.OpenAsync(existingMap.MapFullPath).ConfigureAwait(false);
                        if (package.Maps.Count > 0)
                        {
                            {
                                var basemap = package.Maps.First().Basemap.Clone();
                                return basemap;
                            }
                        }
                        break;
                    }
                case ".tpk":
                    {
                        TileCache titleCache = new TileCache(existingMap.MapFullPath);
                        var layer = new ArcGISTiledLayer(titleCache)
                        {
                            //zoom to any level
                            //if area is out of the map
                            // should be available to navigate

                            MinScale = 100000000,
                            MaxScale = 1
                        };

                        await layer.LoadAsync().ConfigureAwait(false);
                        return new Basemap(layer);

                    }
                case ".tif":
                    {
                        Raster raster = new Raster(existingMap.MapFullPath);
                        RasterLayer newRasterLayer = new RasterLayer(raster);
                        await newRasterLayer.LoadAsync().ConfigureAwait(false);

                        //add error display
                        //
                        if (newRasterLayer.SpatialReference.IsProjected)
                        {
                            return new Basemap(newRasterLayer);
                        }
                        break;
                    }
            }

            return null;
        }

        public async Task UpdateBaseMap()
        {
            var existingMap = this.AvailableMaps.FirstOrDefault(x => x.MapName == this.SelectedMap);

            if (existingMap != null)
            {
                var basemap = await GetBaseMap(existingMap);

                this.Map.Basemap = basemap;

                if (basemap?.BaseLayers[0]?.FullExtent != null)
                    await MapView.SetViewpointGeometryAsync(basemap.BaseLayers[0].FullExtent);
            }
        }

        private string selectedMap;
        public string SelectedMap
        {
            get => this.selectedMap;
            set => this.RaiseAndSetIfChanged(ref this.selectedMap, value);
        }

        private List<string> mapsList = new List<string>();
        public List<string> MapsList
        {
            get => this.mapsList;
            set => this.RaiseAndSetIfChanged(ref this.mapsList, value);
        }

        public IMvxAsyncCommand RotateMapToNorth => new MvxAsyncCommand(async () =>
            await this.MapView?.SetViewpointRotationAsync(0));

        public IMvxAsyncCommand ZoomMapIn => new MvxAsyncCommand(async () =>
            await this.MapView?.SetViewpointScaleAsync(this.MapView.MapScale / 1.3));

        public IMvxAsyncCommand ZoomMapOut => new MvxAsyncCommand(async () =>
            await this.MapView?.SetViewpointScaleAsync(this.MapView.MapScale * 1.3));

        private bool isLocationServiceSwitchEnabled = true;
        private IMapService mapService;

        public bool IsLocationServiceSwitchEnabled
        {
            get => this.isLocationServiceSwitchEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isLocationServiceSwitchEnabled, value);
        }

        public IMvxAsyncCommand SwitchLocatorCommand => new MvxAsyncCommand(async () =>
        {
            if (!IsLocationServiceSwitchEnabled)
                return;

            //try to workaround Esri crash with location service
            //Esri case 02209395
            try
            {
                IsLocationServiceSwitchEnabled = false;

                if (!this.MapView.LocationDisplay.IsEnabled)
                    this.MapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;

                //try to stop service first to avoid crash
                await this.MapView.LocationDisplay.DataSource.StopAsync();

                await this.MapView.LocationDisplay.DataSource.StartAsync();
                this.MapView.LocationDisplay.IsEnabled = true;
                this.MapView.LocationDisplay.LocationChanged += LocationDisplayOnLocationChanged;
            }
            catch (Exception exc)
            {
                logger.Error("Error occurred on map location start.", exc);
                IsLocationServiceSwitchEnabled = false;
            }
        });

        private void LocationDisplayOnLocationChanged(object sender, Location e)
        {
            //show only once
            this.MapView.LocationDisplay.LocationChanged -= LocationDisplayOnLocationChanged;

            if (e.Position == null) { return; }

            var extent = this.MapView.Map.Basemap.BaseLayers[0].FullExtent;

            var point = GeometryEngine.Project(e.Position, extent.SpatialReference);

            if (!GeometryEngine.Contains(extent, point))
            {
                this.userInteractionService.ShowToast(UIResources.AreaMap_LocationOutOfBoundaries);
            }
        }

        private MvxObservableCollection<MapDescription> availableMaps = new MvxObservableCollection<MapDescription>();
        private IFileSystemAccessor fileSystemAccessor;

        public MvxObservableCollection<MapDescription> AvailableMaps
        {
            get => this.availableMaps;
            protected set => this.RaiseAndSetIfChanged(ref this.availableMaps, value);
        }

        public IMvxCommand NavigateToDashboardCommand => 
            new MvxAsyncCommand(async () => await this.viewModelNavigationService.NavigateToDashboardAsync());
        
        public void Dispose()
        {
            if(this.MapView?.LocationDisplay!= null )
                this.MapView.LocationDisplay.LocationChanged -= LocationDisplayOnLocationChanged;

            if(MapView != null)
                MapView.GeoViewTapped -= OnMapViewTapped;
        }

    }
}
