using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.UI.Shared.Enumerator.Services.Internals.MapService;

namespace WB.UI.Shared.Extensions.CustomServices.AreaEditor
{
    public class AreaEditorViewModel : BaseViewModel
    {
        public event Action<AreaEditorResult> OnAreaEditCompleted;

        readonly ILogger logger;
        private readonly IMapService mapService;
        private readonly IUserInteractionService userInteractionService;

        public AreaEditorViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IMapService mapService,
            IUserInteractionService userInteractionService,
            ILogger logger)
            : base(principal, viewModelNavigationService)
        {
            this.userInteractionService = userInteractionService;
            this.mapService = mapService;
            this.logger = logger;
        }

        public override void Load()
        {
            this.AvailableMaps = new MvxObservableCollection<MapDescription>(this.mapService.GetAvailableMaps());
            this.MapsList = this.AvailableMaps.Select(x => x.MapName).ToList();

            if (this.AvailableMaps.Count == 0) return;
            this.ReloadMap();
        }

        private void ReloadMap()
        {
            if (!string.IsNullOrEmpty(this.MapName) && this.MapsList.Contains(this.MapName))
            {
                this.SelectedMap = this.MapName;
            }
            else
            {
                this.SelectedMap = this.MapsList.FirstOrDefault();
            }
        }

        private MvxObservableCollection<MapDescription> availableMaps = new MvxObservableCollection<MapDescription>();
        public MvxObservableCollection<MapDescription> AvailableMaps
        {
            get => this.availableMaps;
            protected set => this.RaiseAndSetIfChanged(ref this.availableMaps, value);
        }
        

        private List<string> mapsList = new List<string>();
        public List<string> MapsList
        {
            get => this.mapsList;
            set => this.RaiseAndSetIfChanged(ref this.mapsList, value);
        }

        private string selectedMap;
        public string SelectedMap
        {
            get => this.selectedMap;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedMap, value);

                var existingMap = this.AvailableMaps.FirstOrDefault(x => x.MapName == value);

                if (existingMap != null)
                {
                    this.UpdateBaseMap(existingMap.MapFullPath);
                }
            }
        }

        public void UpdateBaseMap(string pathToMap)
        {
            if (pathToMap != null)
            {
                //woraround to fix size of map
                //on map reload
                if (MapView?.LocationDisplay != null)
                    this.MapView.LocationDisplay.IsEnabled = false;

                TileCache titleCache = new TileCache(pathToMap);
                var layer = new ArcGISTiledLayer(titleCache)
                {
                    //zoom to any level
                    //if area is out of the map
                    // should be available to navigate
                    MinScale = 100000000,
                    MaxScale = 1
                };

                this.Map = new Map
                {
                    Basemap = new Basemap(layer),
                    MinScale = 100000000,
                    MaxScale = 1
                };
            }
            else
            {
                this.Map = null;
            }
        }

        public void Init(string geometry, string mapName)
        {
            this.MapName = mapName;

            if (!string.IsNullOrEmpty(geometry))
            {
                this.Geometry = Geometry.FromJson(geometry);

                this.GeometryArea = GeometryEngine.AreaGeodetic(this.Geometry).ToString("#.##");
                this.GeometryLength = GeometryEngine.LengthGeodetic(this.Geometry).ToString("#.##");
            }
        }

        private Geometry Geometry { set; get; } = null;
        public string MapName { set; get; }

        private Map map;
        public Map Map
        {
            get => this.map;
            set => this.RaiseAndSetIfChanged(ref this.map, value);
        }

        private MapView mapView;
        public MapView MapView
        {
            get => this.mapView;
            set
            {
                this.mapView = value;
                if (this.mapView != null)
                {
                    this.mapView.ViewAttachedToWindow +=
                        delegate
                        {
                            if (this.StartEditAreaCommand.CanExecute())
                                this.StartEditAreaCommand.Execute();
                        };
                }
            }
        }
       
        public IMvxCommand SwitchMapCommand => new MvxCommand<MapDescription>((MapDescription map) => { this.SelectedMap = map.MapName; });

       
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

        public IMvxCommand CancelCommand => new MvxCommand(() =>
        {
            var handler = this.OnAreaEditCompleted;
            handler?.Invoke(null);
            Close(this);

        });

        public IMvxCommand SwitchLocatorCommand => new MvxCommand(() =>
        {
            if (!IsLocationServiceSwitchEnabled)
                return;

            //temporary catch for KP-9486
            //esri was notified
            try
            {
                IsLocationServiceSwitchEnabled = false;

                if (!this.MapView.LocationDisplay.IsEnabled)
                    this.MapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;

                if (!this.MapView.LocationDisplay.IsEnabled && !this.MapView.LocationDisplay.Started
                    && (this.MapView.LocationDisplay.DataSource != null && !this.MapView.LocationDisplay.DataSource.IsStarted))
                        this.MapView.LocationDisplay.IsEnabled = true;
                else
                    this.MapView.LocationDisplay.IsEnabled = false;
            }
            catch (ArgumentException exc)
            {
                logger.Error("Error occured on map location switch.", exc);
            }
            finally
            {
                //workaround for maps location service error
                Task.Run(() =>
                {
                    Thread.Sleep(5000);
                    IsLocationServiceSwitchEnabled = true;
                });
            }

        });

        public IMvxCommand HidePanelComand => new MvxCommand(() =>
        {
            IsPanelVisible = false;
        });

        public IMvxCommand ShowPanelCommand => new MvxCommand(() =>
        {
            IsPanelVisible = !IsPanelVisible;
        });

        

        public IMvxAsyncCommand UpdateMapsCommand => new MvxAsyncCommand(async () =>
        {
            try
            {
                if (!this.IsInProgress)
                {
                    this.IsInProgress = true;
                    this.cancellationTokenSource = new CancellationTokenSource();
                    await this.mapService.SyncMaps(this.cancellationTokenSource.Token).ConfigureAwait(false);

                    this.AvailableMaps = new MvxObservableCollection<MapDescription>(this.mapService.GetAvailableMaps());
                    this.MapsList = this.AvailableMaps.Select(x => x.MapName).ToList();

                    if (this.Map == null)
                    {
                        this.ReloadMap();
                        this.StartEditAreaCommand.Execute();
                    }
                }
                else
                {
                    if (this.cancellationTokenSource != null && this.cancellationTokenSource.Token.CanBeCanceled)
                        this.cancellationTokenSource.Cancel();
                }
            }
            catch (Exception exc)
            {
                logger.Error("Error occured on map update.", exc);
                this.userInteractionService.ShowToast(UIResources.AreaMap_UpdateFailed);
            }
            finally
            {
                this.IsInProgress = false;
            }
        });

        private CancellationTokenSource cancellationTokenSource;

        public IMvxAsyncCommand StartEditAreaCommand => new MvxAsyncCommand(async () =>
        {
            if (this.IsEditing || this.Map == null)
                return;

            this.IsEditing = true;
            try
            {
                this.MapView.SketchEditor.GeometryChanged += delegate(object sender, GeometryChangedEventArgs args)
                {
                    var geometry = args.NewGeometry;
                    bool isCanCalculateAreaDimensions = IsCanCalculateAreaDimensions(geometry);
                    try
                    {
                        this.GeometryArea = (isCanCalculateAreaDimensions ? GeometryEngine.AreaGeodetic(geometry) : 0).ToString("#.##");
                        this.GeometryLength = (isCanCalculateAreaDimensions ? GeometryEngine.LengthGeodetic(geometry) : 0).ToString("#.##");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("LOG MESSAGE EXCEPTION");
                        Console.WriteLine(e);
                        Console.WriteLine(geometry.ToJson());
                        throw;
                    }

                    this.CanUndo =
                        this.MapView.SketchEditor.UndoCommand.CanExecute(this.MapView.SketchEditor.UndoCommand);
                    this.CanSave =
                        this.MapView.SketchEditor.CompleteCommand.CanExecute(this.MapView.SketchEditor.CompleteCommand);
                };

                Geometry result;
                if (this.Geometry == null)
                {
                    await this.MapView.SetViewpointRotationAsync(0).ConfigureAwait(false); //workaround to fix Map is not prepared.
                    result = await this.MapView.SketchEditor.StartAsync(SketchCreationMode.Polygon, true).ConfigureAwait(false);
                }
                else
                {
                    await this.MapView.SetViewpointGeometryAsync(this.Geometry, 120).ConfigureAwait(false);
                    result = await this.MapView.SketchEditor.StartAsync(this.Geometry, SketchCreationMode.Polygon)
                        .ConfigureAwait(false);
                }

                var position = this.MapView.LocationDisplay.Location.Position;
                double? dist = null;
                if (position != null)
                {
                    var point = new MapPoint(position.X, position.Y, position.Z, this.MapView.SpatialReference);
                    dist = GeometryEngine.Distance(result, point);
                }

                //project to geocoordinates
                SpatialReference reference = new SpatialReference(4326);
                var projectedPolygon = GeometryEngine.Project(result, reference) as Polygon;
                
                string coordinates = string.Empty;
                if (projectedPolygon != null)
                {
                    var tco = projectedPolygon.Parts[0].Points.Select(x => $"{x.X.ToString(CultureInfo.InvariantCulture)},{x.Y.ToString(CultureInfo.InvariantCulture)}").ToList();
                    coordinates = string.Join(";", tco);
                }

                var resultArea = new AreaEditorResult()
                {
                    Geometry = result?.ToJson(),
                    MapName = this.SelectedMap,
                    Coordinates = coordinates,
                    Area = GeometryEngine.AreaGeodetic(result),
                    Length = GeometryEngine.LengthGeodetic(result),
                    DistanceToEditor = dist
                };

                //save
                var handler = this.OnAreaEditCompleted;
                handler?.Invoke(resultArea);
            }
            finally
            {
                this.IsEditing = false;
                Close(this);
            }
        });

        private bool IsCanCalculateAreaDimensions(Geometry geometry)
        {
            if (geometry == null)
                return false;

            if (geometry.GeometryType != GeometryType.Polygon || geometry.Dimension != GeometryDimension.Area)
                return false;

            var polygon = geometry as Polygon;
            if (polygon == null)
                return false;

            if (polygon.Parts.Count < 1)
                return false;

            var readOnlyPart = polygon.Parts[0];
            if (readOnlyPart.PointCount < 3)
                return false;

            var groupedPoints = from point in readOnlyPart.Points
                group point by new { X = point.X, Y = point.Y } into xyPoint
                select new { X = xyPoint.Key.X, Y = xyPoint.Key.Y, Count = xyPoint.Count() };

            if (groupedPoints.Count() < 3)
                return false;

            return true;
        }

        private string geometryArea = "0";
        public string GeometryArea
        {
            get => this.geometryArea;
            set => this.RaiseAndSetIfChanged(ref this.geometryArea, value);
        }

        private string geometryLength = "0";
        public string GeometryLength
        {
            get => this.geometryLength;
            set => this.RaiseAndSetIfChanged(ref this.geometryLength, value);
        }

        private void BtnUndo()
        {
            var command = this.MapView?.SketchEditor.UndoCommand;
            if (this.MapView?.SketchEditor?.UndoCommand.CanExecute(command) ?? false)
                this.MapView.SketchEditor.UndoCommand.Execute(command);
        }

        private void BtnCancelCommand()
        {
            var command = this.MapView?.SketchEditor.UndoCommand;
            while (this.MapView?.SketchEditor?.UndoCommand.CanExecute(command) ?? false)
            {
                this.MapView.SketchEditor.UndoCommand.Execute(command);
            }
        }

        public IMvxCommand UndoCommand => new MvxCommand(this.BtnUndo);
        public IMvxCommand CancelEditCommand => new MvxCommand(this.BtnCancelCommand);

        private bool isEditing;
        public bool IsEditing
        {
            get => this.isEditing;
            set => this.RaiseAndSetIfChanged(ref this.isEditing, value);
        }

        private bool canUndo;
        public bool CanUndo
        {
            get => this.canUndo;
            set => this.RaiseAndSetIfChanged(ref this.canUndo, value);
        }

        private bool canSave;
        public bool CanSave
        {
            get => this.canSave;
            set => this.RaiseAndSetIfChanged(ref this.canSave, value);
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set => this.RaiseAndSetIfChanged(ref this.isInProgress, value);
        }

        private bool isPanelVisible;
        public bool IsPanelVisible
        {
            get => this.isPanelVisible;
            set => this.RaiseAndSetIfChanged(ref this.isPanelVisible, value);
        }

        private bool isLocationServiceSwitchEnabled = true; 
        public bool IsLocationServiceSwitchEnabled
        {
            get => this.isLocationServiceSwitchEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isLocationServiceSwitchEnabled, value);
        }
    }
}