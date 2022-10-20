using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.UI.Shared.Extensions.Entities;
using WB.UI.Shared.Extensions.Extensions;
using WB.UI.Shared.Extensions.Services;
using GeometryType = WB.Core.SharedKernels.Questionnaire.Documents.GeometryType;

namespace WB.UI.Shared.Extensions.ViewModels
{
    public class GeographyEditorViewModel : BaseMapInteractionViewModel<GeographyEditorViewModelArgs>
    {
        protected const string NeighborsLayerName = "neighbors";
        
        public Action<AreaEditorResult> OnAreaEditCompleted;

        public GeographyEditorViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService, 
            IMapService mapService, IUserInteractionService userInteractionService, ILogger logger,
            IEnumeratorSettings enumeratorSettings, IMapUtilityService mapUtilityService, 
            IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher) 
            : base(principal, viewModelNavigationService, mapService, userInteractionService, logger, 
                enumeratorSettings, mapUtilityService, mainThreadAsyncDispatcher){}

        private int? RequestedAccuracy { get; set; }
        public int? RequestedFrequency { get; set; }
        public GeometryInputMode RequestedGeometryInputMode { get; set; }
        public GeometryNeighbor[] GeographyNeighbors { get; set; }
        private Geometry Geometry { set; get; }
        public string MapName { set; get; }
        public string Title { set; get; }
        private GeometryType? requestedGeometryType;

        public bool IsManual => RequestedGeometryInputMode == GeometryInputMode.Manual;
        public bool IsCanAddPoint => false;
        
        public override void Prepare(GeographyEditorViewModelArgs parameter)
        {
            this.MapName = parameter.MapName;
            this.Title = parameter.Title;
            this.requestedGeometryType = parameter.RequestedGeometryType;
            this.RequestedAccuracy = parameter.RequestedAccuracy;
            this.RequestedFrequency = parameter.RequestedFrequency;
            this.RequestedGeometryInputMode = parameter.RequestedGeometryInputMode ?? GeometryInputMode.Manual;
            this.GeographyNeighbors = parameter.GeographyNeighbors
                .Select(n => new GeometryNeighbor()
                {
                    Id = n.Id,
                    Title = n.Title,
                    Geometry = Geometry.FromJson(n.Geometry),
                }).ToArray();

            if (string.IsNullOrEmpty(parameter.Geometry)) return;

            this.Geometry = Geometry.FromJson(parameter.Geometry);
            this.UpdateLabels(this.Geometry);
        }


        public IMvxAsyncCommand CancelCommand => new MvxAsyncCommand(async () =>
        {
            var handler = this.OnAreaEditCompleted;
            handler?.Invoke(null);
            await this.NavigationService.Close(this);
        });
        
        public IMvxAsyncCommand StartEditAreaCommand => new MvxAsyncCommand(async () => await EditGeometry());

        private async Task EditGeometry()
        {
            this.IsEditing = true;

            try
            {
                this.MapView.SketchEditor.GeometryChanged += async delegate (object sender, GeometryChangedEventArgs args)
                {
                    var geometry = args.NewGeometry;
                    try
                    {
                        this.UpdateLabels(geometry);
                        await UpdateDrawNeighborsAsync(geometry, this.GeographyNeighbors).ConfigureAwait(false);
                    }
                    catch
                    {
                    }

                    this.CanUndo = this.MapView.SketchEditor.UndoCommand.CanExecute(this.MapView.SketchEditor.UndoCommand);
                    this.CanSave = this.MapView.SketchEditor.CompleteCommand.CanExecute(this.MapView.SketchEditor.CompleteCommand);
                };

                if (this.Geometry == null)
                {
                    await this.MapView.SetViewpointRotationAsync(0).ConfigureAwait(false); //workaround to fix Map is not prepared Esri error.
                }
                else
                {
                    if (this.Geometry.GeometryType == Esri.ArcGISRuntime.Geometry.GeometryType.Point)
                        await this.MapView.SetViewpointCenterAsync(this.Geometry as MapPoint).ConfigureAwait(false);
                    else
                        await this.MapView.SetViewpointGeometryAsync(this.Geometry, 120).ConfigureAwait(false);
                }

                await DrawNeighborsAsync(this.requestedGeometryType, this.Geometry, this.GeographyNeighbors).ConfigureAwait(false);
                var result = await GetGeometry(this.requestedGeometryType, this.Geometry).ConfigureAwait(false);

                var position = this.MapView?.LocationDisplay?.Location?.Position;
                double? distanceToEditor = null;
                if (position != null)
                {
                    var point = GeometryEngine.Project(position, this.MapView.SpatialReference);
                    distanceToEditor = GeometryEngine.Distance(result, point);
                }

                var resultArea = new AreaEditorResult()
                {
                    Geometry = result?.ToJson(),
                    MapName = this.SelectedMap,
                    Coordinates = GeometryHelper.GetProjectedCoordinates(result),
                    Area = GeometryHelper.GetGeometryArea(result),
                    Length = GeometryHelper.GetGeometryLength(result),
                    DistanceToEditor = distanceToEditor,
                    NumberOfPoints = GeometryHelper.GetGeometryPointsCount(result),
                    RequestedAccuracy = RequestedAccuracy
                };

                //save
                var handler = this.OnAreaEditCompleted;
                handler?.Invoke(resultArea);
            }
            finally
            {
                this.IsEditing = false;
                if(this.MapView?.LocationDisplay != null)
                    this.MapView.LocationDisplay.LocationChanged -= LocationDisplayOnLocationChanged;
                
                await this.NavigationService.Close(this);
            }
        }

        private void UpdateLabels(Geometry geometry)
        {
            var area = GeometryHelper.GetGeometryArea(geometry);
            var length = GeometryHelper.GetGeometryLength(geometry);

            this.GeometryArea = area > 0 ? string.Format(UIResources.AreaMap_AreaFormat, area.ToString("#.##")) : string.Empty;
            this.GeometryLengthLabel = length > 0 ? string.Format(
                this.requestedGeometryType == GeometryType.Polygon
                    ? UIResources.AreaMap_PerimeterFormat
                    : UIResources.AreaMap_LengthFormat, length.ToString("#.##")) : string.Empty;
        }

        private async Task DrawNeighborsAsync(GeometryType? geometryType, Geometry geometry, GeometryNeighbor[] neighbors)
        {
            if (neighbors == null || neighbors.Length == 0)
                return;

            var gType = geometryType ?? GeometryType.Polygon;
            var esriGeometryType = gType.ToEsriGeometryType();

            IEnumerable<Field> fields = new Field[]
            {
                new Field(FieldType.Text, "name", "Name", 50)
            };
            var neighborsFeatureCollectionTable = new FeatureCollectionTable(fields, esriGeometryType, this.MapView.SpatialReference);
            neighborsFeatureCollectionTable.Renderer = CreateRenderer(gType, Color.Blue);
            var overlappingNeighborsFeatureCollectionTable = new FeatureCollectionTable(fields, esriGeometryType, this.MapView.SpatialReference);
            overlappingNeighborsFeatureCollectionTable.Renderer = CreateRenderer(gType, Color.Red);

            List<string> overlappingTitles = new List<string>();

            for (int i = 0; i < neighbors.Length; i++)
            {
                var neighbor = neighbors[i];
                var featureCollectionTable = neighborsFeatureCollectionTable;
                
                if (geometry != null)
                {
                    var isOverlapping = !GeometryEngine.Disjoint(geometry, neighbor.Geometry);
                    neighbor.IsOverlapping = isOverlapping;
                    
                    if (isOverlapping)
                        overlappingTitles.Add(neighbor.Title);
                    
                    featureCollectionTable = isOverlapping
                        ? overlappingNeighborsFeatureCollectionTable
                        : neighborsFeatureCollectionTable;
                }

                var feature = featureCollectionTable.CreateFeature();
                feature.Geometry = neighbor.Geometry;
                neighbor.Feature = feature;
                await featureCollectionTable.AddFeatureAsync(feature);
            }

            FeatureCollection featuresCollection = new FeatureCollection();
            featuresCollection.Tables.Add(neighborsFeatureCollectionTable);
            featuresCollection.Tables.Add(overlappingNeighborsFeatureCollectionTable);
            FeatureCollectionLayer featureCollectionLayer = new FeatureCollectionLayer(featuresCollection)
            {
                Name = NeighborsLayerName
            };

            var existedLayer = this.MapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == featureCollectionLayer.Name);
            if (existedLayer != null)
                this.MapView.Map.OperationalLayers.Remove(existedLayer);
            
            this.MapView.Map.OperationalLayers.Add(featureCollectionLayer);

            UpdateWarningMessage(overlappingTitles);
        }

        private void UpdateWarningMessage(List<string> overlappingTitles)
        {
            if (overlappingTitles.Count > 0)
            {
                var maxTitlesDisplay = 2;
                var message = string.Format(UIResources.AreaMap_OverlapsWith, this.Title)
                              + "\r\n - " + string.Join("\r\n - ", overlappingTitles.Take(maxTitlesDisplay).ToArray());
                if (overlappingTitles.Count > maxTitlesDisplay)
                    message += "\r\n" + string.Format(UIResources.AreaMap_OverlapsWithOther,
                        overlappingTitles.Count - maxTitlesDisplay);
                this.Warning = message;
            }

            IsWarningVisible = overlappingTitles.Count > 0;
        }

        private async Task UpdateDrawNeighborsAsync(Geometry geometry, GeometryNeighbor[] neighbors)
        {
            if (neighbors == null || neighbors.Length == 0)
                return;

            var existedLayer = this.MapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == NeighborsLayerName);
            if (existedLayer == null)
                return;
            
            var featureCollectionLayer = (FeatureCollectionLayer)existedLayer;
            var featureCollectionTables = featureCollectionLayer.FeatureCollection.Tables;
            var neighborsFeatureCollectionTable = featureCollectionTables[0];
            var overlappingNeighborsFeatureCollectionTable = featureCollectionTables[1];

            List<string> overlappingTitles = new List<string>();

            foreach (var neighbor in neighbors)
            {
                var isOverlapping = geometry != null && !GeometryEngine.Disjoint(geometry, neighbor.Geometry);
                if (isOverlapping)
                    overlappingTitles.Add(neighbor.Title);
                
                if (neighbor.IsOverlapping != isOverlapping)
                {
                    var featureCollectionTable = isOverlapping
                        ? overlappingNeighborsFeatureCollectionTable
                        : neighborsFeatureCollectionTable;
                    var feature = featureCollectionTable.CreateFeature();
                    feature.Geometry = neighbor.Geometry;
                    await featureCollectionTable.AddFeatureAsync(feature);
                    
                    var oldFeatureCollectionTable = !isOverlapping
                        ? overlappingNeighborsFeatureCollectionTable
                        : neighborsFeatureCollectionTable;
                    await oldFeatureCollectionTable.DeleteFeatureAsync(neighbor.Feature);
                    
                    neighbor.Feature = feature;
                    neighbor.IsOverlapping = isOverlapping;
                }
            }

            UpdateWarningMessage(overlappingTitles);
        }

        private async Task<Geometry> GetGeometry(GeometryType? geometryType, Geometry geometry)
        {
            switch (geometryType)
            {
                case GeometryType.Polyline:
                    {
                        IsGeometryLengthVisible = true;
                        IsGeometryAreaVisible = false;
                        
                        return geometry == null
                            ? await this.MapView.SketchEditor.StartAsync(SketchCreationMode.Polyline).ConfigureAwait(false)
                            : await this.MapView.SketchEditor.StartAsync(geometry, SketchCreationMode.Polyline)
                                .ConfigureAwait(false);
                    }
                case GeometryType.Point:
                    {
                        IsGeometryLengthVisible = false;
                        IsGeometryAreaVisible = false;

                        return geometry == null
                            ? await this.MapView.SketchEditor.StartAsync(SketchCreationMode.Point).ConfigureAwait(false)
                            : await this.MapView.SketchEditor.StartAsync(geometry, SketchCreationMode.Point)
                                .ConfigureAwait(false);
                    }
                case GeometryType.Multipoint:
                    {
                        IsGeometryLengthVisible = false;
                        IsGeometryAreaVisible = false;

                        this.MapView.SketchEditor.Style.MidVertexSymbol = null;
                        this.MapView.SketchEditor.Style.LineSymbol = null;

                        return geometry == null ?
                            await this.MapView.SketchEditor.StartAsync(SketchCreationMode.Polyline).ConfigureAwait(false) :
                            await this.MapView.SketchEditor.StartAsync(geometry, SketchCreationMode.Polyline).ConfigureAwait(false);
                    }
                default:
                    {
                        IsGeometryLengthVisible = true;
                        IsGeometryAreaVisible = true;

                        return geometry == null
                            ? await this.MapView.SketchEditor.StartAsync(SketchCreationMode.Polygon).ConfigureAwait(false)
                            : await this.MapView.SketchEditor.StartAsync(geometry, SketchCreationMode.Polygon)
                                .ConfigureAwait(false);
                    }
            }
        }
        
        private Renderer CreateRenderer(GeometryType rendererType, Color color)
        {
            Symbol sym = rendererType switch
            {
                GeometryType.Point => new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, color, 18),
                GeometryType.Multipoint => new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, color, 18),
                GeometryType.Polyline => new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, color, 2),
                GeometryType.Polygon => new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, color, 2),
                _ => null
            };

            return new SimpleRenderer(sym);
        }

        public override Task OnMapLoaded()
        {
            return EditGeometry();
        }

        public override MapDescription GetSelectedMap(MvxObservableCollection<MapDescription> mapsToSelectFrom)
        {
            MapDescription mapToLoad = null;
            
            if (!string.IsNullOrEmpty(this.MapName))
            {
                mapToLoad = mapsToSelectFrom.FirstOrDefault(x => x.MapName == this.MapName);
            }

            if (!string.IsNullOrEmpty(this.LastMap))
            {
                mapToLoad = mapsToSelectFrom.FirstOrDefault(x => x.MapName == LastMap);
            }
            
            mapToLoad ??= mapsToSelectFrom.FirstOrDefault(x => x.MapType == MapType.LocalFile);
                
            return mapToLoad ?? mapsToSelectFrom.First();
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

        private bool isCollecting = false;
        public IMvxCommand StartCollectingCommand => new MvxCommand(this.StartCollecting);
        private void StartCollecting()
        {
            isCollecting = !isCollecting;
            //call SwitchLocatorCommand
        }
        
        public IMvxCommand AddPointCommand => new MvxCommand(this.AddPoint);
        private void AddPoint()
        {
            //call SwitchLocatorCommand
        }

        private string warning;
        public string Warning
        {
            get => this.warning;
            set => this.RaiseAndSetIfChanged(ref this.warning, value);
        }
 
        private bool isWarningVisible;
        public bool IsWarningVisible
        {
            get => this.isWarningVisible;
            set => this.RaiseAndSetIfChanged(ref this.isWarningVisible, value);
        }

        private string geometryArea;
        public string GeometryArea
        {
            get => this.geometryArea;
            set => this.RaiseAndSetIfChanged(ref this.geometryArea, value);
        }

        private string geometryLengthLabel;
        public string GeometryLengthLabel
        {
            get => this.geometryLengthLabel;
            set => this.RaiseAndSetIfChanged(ref this.geometryLengthLabel, value);
        }

        

        private bool isEditing;
        public bool IsEditing
        {
            get => this.isEditing;
            set => this.RaiseAndSetIfChanged(ref this.isEditing, value);
        }

        private bool isGeometryAreaVisible;
        public bool IsGeometryAreaVisible
        {
            get => this.isEditing;
            set => this.RaiseAndSetIfChanged(ref this.isGeometryAreaVisible, value);
        }

        private bool isGeometryLengthVisible;
        public bool IsGeometryLengthVisible
        {
            get => this.isEditing;
            set => this.RaiseAndSetIfChanged(ref this.isGeometryLengthVisible, value);
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

        
        public IMvxCommand SwitchPanelCommand => new MvxCommand(() =>
        {
            IsPanelVisible = !IsPanelVisible;
        });
        
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
    }
}
