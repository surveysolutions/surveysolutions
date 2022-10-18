using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.UI.Shared.Extensions.Entities;
using WB.UI.Shared.Extensions.Services;
using GeometryType = WB.Core.SharedKernels.Questionnaire.Documents.GeometryType;

namespace WB.UI.Shared.Extensions.ViewModels
{
    public class GeographyEditorViewModel : BaseMapInteractionViewModel<GeographyEditorViewModelArgs>
    {
        public Action<AreaEditorResult> OnAreaEditCompleted;

        public GeographyEditorViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService, 
            IMapService mapService, IUserInteractionService userInteractionService, ILogger logger, 
            IFileSystemAccessor fileSystemAccessor, IEnumeratorSettings enumeratorSettings,
            IMapUtilityService mapUtilityService, IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher) 
            : base(principal, viewModelNavigationService, mapService, userInteractionService, logger, 
                fileSystemAccessor, enumeratorSettings, mapUtilityService, mainThreadAsyncDispatcher)
        {
        }

        private int? RequestedAccuracy { get; set; }
        public int? RequestedFrequency { get; set; }
        public GeometryInputMode RequestedGeometryInputMode { get; set; }
        
        private Geometry Geometry { set; get; }
        public string MapName { set; get; }
        private WB.Core.SharedKernels.Questionnaire.Documents.GeometryType? requestedGeometryType;
        
        public override void Prepare(GeographyEditorViewModelArgs parameter)
        {
            this.MapName = parameter.MapName;
            this.requestedGeometryType = parameter.RequestedGeometryType;
            this.RequestedAccuracy = parameter.RequestedAccuracy;
            this.RequestedFrequency = parameter.RequestedFrequency;
            this.RequestedGeometryInputMode = parameter.RequestedGeometryInputMode ?? GeometryInputMode.Manual;

            if (string.IsNullOrEmpty(parameter.Geometry)) return;

            this.Geometry = Geometry.FromJson(parameter.Geometry);
            this.UpdateLabels(this.Geometry);
        }

        public IMvxAsyncCommand CancelCommand => new MvxAsyncCommand(async () =>
        {
            var handler = this.OnAreaEditCompleted;
            handler?.Invoke(null);
            await this.navigationService.Close(this);
        });
        
        public IMvxAsyncCommand StartEditAreaCommand => new MvxAsyncCommand(async () => await EditGeometry());

        private async Task EditGeometry()
        {
            this.IsEditing = true;

            try
            {
                this.MapView.SketchEditor.GeometryChanged += delegate (object sender, GeometryChangedEventArgs args)
                {
                    var geometry = args.NewGeometry;
                    try
                    {
                        this.UpdateLabels(geometry);
                    }
                    catch
                    {
                    }

                    this.CanUndo =
                        this.MapView.SketchEditor.UndoCommand.CanExecute(this.MapView.SketchEditor.UndoCommand);
                    this.CanSave =
                        this.MapView.SketchEditor.CompleteCommand.CanExecute(this.MapView.SketchEditor.CompleteCommand);
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
                    Coordinates = GetProjectedCoordinates(result),
                    Area = GetGeometryArea(result),
                    Length = GetGeometryLength(result),
                    DistanceToEditor = distanceToEditor,
                    NumberOfPoints = GetGeometryPointsCount(result),
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
                
                await this.navigationService.Close(this);
            }
        }

        private string GetProjectedCoordinates(Geometry result)
        {
            string coordinates = string.Empty;
            if(result == null)
                return coordinates;
            
            //project to geo-coordinates
            SpatialReference reference = SpatialReference.Create(4326);

            switch (result.GeometryType)
            {
                case Esri.ArcGISRuntime.Geometry.GeometryType.Polygon:
                    var polygonCoordinates = (result as Polygon).Parts[0].Points
                        .Select(point => GeometryEngine.Project(point, reference) as MapPoint)
                        .Select(coordinate => $"{coordinate.X.ToString(CultureInfo.InvariantCulture)},{coordinate.Y.ToString(CultureInfo.InvariantCulture)}").ToList();
                    coordinates = string.Join(";", polygonCoordinates);
                    break;
                case Esri.ArcGISRuntime.Geometry.GeometryType.Point:
                    var projected = GeometryEngine.Project(result as MapPoint, reference) as MapPoint;
                    coordinates = $"{projected.X.ToString(CultureInfo.InvariantCulture)},{projected.Y.ToString(CultureInfo.InvariantCulture)}";
                    break;
                case Esri.ArcGISRuntime.Geometry.GeometryType.Polyline:
                    var polylineCoordinates = (result as Polyline).Parts[0].Points
                        .Select(point => GeometryEngine.Project(point, reference) as MapPoint)
                        .Select(coordinate => $"{coordinate.X.ToString(CultureInfo.InvariantCulture)},{coordinate.Y.ToString(CultureInfo.InvariantCulture)}").ToList();
                    coordinates = string.Join(";", polylineCoordinates);
                    break;
            }

            return coordinates;
        }

        private double GetGeometryArea(Geometry geometry)
        {
            bool doesGeometrySupportDimensionsCalculation = DoesGeometrySupportAreaCalculation(geometry);
            return doesGeometrySupportDimensionsCalculation ? GeometryEngine.AreaGeodetic(geometry) : 0;
        }

        private double GetGeometryLength(Geometry geometry)
        {
            bool doesGeometrySupportDimensionsCalculation = DoesGeometrySupportLengthCalculation(geometry);
            return doesGeometrySupportDimensionsCalculation ? GeometryEngine.LengthGeodetic(geometry) : 0;
        }
        
        private bool DoesGeometrySupportLengthCalculation(Geometry geometry)
        {
            if (geometry == null)
                return false;

            if (requestedGeometryType == GeometryType.Multipoint || requestedGeometryType == GeometryType.Point)
                return false;

            return true;
        }
        
        private void UpdateLabels(Geometry geometry)
        {
            var area = this.GetGeometryArea(geometry);
            var length = this.GetGeometryLength(geometry);

            this.GeometryArea = area > 0 ? string.Format(UIResources.AreaMap_AreaFormat, area.ToString("#.##")) : string.Empty;
            this.GeometryLengthLabel = length > 0 ? string.Format(
                this.requestedGeometryType == GeometryType.Polygon
                    ? UIResources.AreaMap_PerimeterFormat
                    : UIResources.AreaMap_LengthFormat, length.ToString("#.##")) : string.Empty;
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

        private int GetGeometryPointsCount(Geometry geometry)
        {
            switch (geometry.GeometryType)
            {
                case Esri.ArcGISRuntime.Geometry.GeometryType.Point:
                    return 1;

                case Esri.ArcGISRuntime.Geometry.GeometryType.Polyline:
                    return (geometry as Polyline).Parts[0].PointCount;

                case Esri.ArcGISRuntime.Geometry.GeometryType.Polygon:
                    return (geometry as Polygon).Parts[0].PointCount;

                case Esri.ArcGISRuntime.Geometry.GeometryType.Multipoint:
                    return (geometry as Multipoint).Points.Count();
                default:
                    return 0;
            }
        }

        private bool DoesGeometrySupportAreaCalculation(Geometry geometry)
        {
            if (geometry == null)
                return false;

            if (geometry.GeometryType != Esri.ArcGISRuntime.Geometry.GeometryType.Polygon || geometry.Dimension != GeometryDimension.Area)
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
