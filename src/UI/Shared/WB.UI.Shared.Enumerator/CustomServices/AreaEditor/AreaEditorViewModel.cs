using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.CustomServices.AreaEditor
{
    public class AreaEditorViewModel : BaseViewModel
    {
        public event Action<AreaEditorResult> OnAreaEditCompleted;

        private IMapService mapService;
        private IUserInteractionService userInteractionService;

        public AreaEditorViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IMapService mapService,
            IUserInteractionService userInteractionService) 
            : base(principal, viewModelNavigationService)
        {
            this.userInteractionService = userInteractionService;
            this.mapService = mapService;
        }

        public override void Load()
        {
            AvailableMaps = mapService.GetAvailableMaps();
            MapsList = AvailableMaps.Keys.ToList();

            if (AvailableMaps.Count != 0)
            {
                if (!string.IsNullOrEmpty(MapName) && AvailableMaps.ContainsKey(MapName))
                {
                    SelectedMap = MapName;
                }
                else
                {
                    SelectedMap = AvailableMaps.FirstOrDefault().Key;
                }
            }
            
            UpdateBaseMap(AvailableMaps[SelectedMap]);
        }

        private Dictionary<string,string> AvailableMaps = new Dictionary<string, string>();

        private List<string> mapsList;
        public List<string> MapsList
        {
            get { return mapsList; }
            set { mapsList = value; RaisePropertyChanged(); }
        }

        private string selectedMap;
        public string SelectedMap
        {
            get { return selectedMap; }
            set
            {
                selectedMap = value;
                RaisePropertyChanged();

                if (AvailableMaps.ContainsKey(value))
                {
                    UpdateBaseMap(AvailableMaps[value]);
                    //this.ShowAllMap();
                }
                
            }
        }

        private void ShowAllMap()
        {
            this.mapView.SetViewpointGeometryAsync(this.mapView.Map?.Basemap.BaseLayers[0].FullExtent).ConfigureAwait(false);
        }

        public override void Start()
        {
            base.Start();
/*
            if(!string.IsNullOrEmpty(Area))
                StartEditAreaCommand.Execute();
*/
        }

        public void UpdateBaseMap(string pathToMap)
        {
            if (pathToMap != null)
            {
                if (this.Map == null)
                    this.Map = new Map();

                TileCache titleCache = new TileCache(pathToMap);
                var layer = new ArcGISTiledLayer(titleCache);

                //zoom to any level
                //if area is out of the map
                // should be available to navigate
                layer.MinScale = 100000000;
                layer.MaxScale = 1;
                //

                var basemap = new Basemap(layer);
                this.Map.Basemap = basemap;
            }
            else
            {
                this.Map = null;
            }
        }
        
        public void Init(string geometry, string mapName, double areaSize)
        {
            Area = geometry;
            MapName = mapName;
            GeometryArea = areaSize;
        }

        public string Area { set; get; }
        public string MapName { set; get; }

        private Map map;
        public Map Map
        {
            get { return this.map; }
            set
            {
                this.map = value;
                RaisePropertyChanged();
            }
        }

        private MapView mapView;
        public MapView MapView {
            set
            {
                this.mapView = value;
                /*if (this.mapView != null)
                {
                    //this.mapView.LayerViewStateChanged
                }*/

                /*this.mapView.SpatialReferenceChanged += (s, e) =>
                    {
                        if(this.mapView.Map != null)
                            this.mapView.SetViewpointGeometryAsync(this.mapView.Map?.Basemap.BaseLayers[0].FullExtent).ConfigureAwait(false);

                        //this.MapView.Map.Basemap.Item.Extent.
                        //this.MapView.SetViewpointAsync(new Viewpoint(geometry));
                        //this.mapView.Map.MinScale = 100000000;
                        //this.mapView.Map.MaxScale = 1;
                    };*/
            }
            get { return this.mapView; }
        }

        //this.mapView.SpatialReferenceChanged += (s, e) => { this.mapView.Map.MinScale = 100000000; this.mapView.Map.MaxScale = 1; };

        public IMvxCommand SaveAreaCommand => new MvxCommand(() =>
        {
            var command = this.MapView.SketchEditor.CompleteCommand;
            if (this.MapView.SketchEditor.CompleteCommand.CanExecute(command))
                this.MapView.SketchEditor.CompleteCommand.Execute(command);
            else
            {
                userInteractionService.ShowToast("No changes we made to be saved");
            }
        });

        public IMvxCommand SwitchLocatorCommand => new MvxCommand(() =>
        {
            if(!this.MapView.LocationDisplay.IsEnabled)
                this.MapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;

            this.MapView.LocationDisplay.IsEnabled = !this.MapView.LocationDisplay.IsEnabled;
        });

        public IMvxCommand UpdateMapsCommand => new MvxCommand(async () =>
        {
            if (!IsInProgress)
            {
                IsInProgress = true;
                cancellationTokenSource = new CancellationTokenSource();
                await mapService.SyncMaps(cancellationTokenSource.Token);

                AvailableMaps = mapService.GetAvailableMaps();
                MapsList = AvailableMaps.Keys.ToList();

                IsInProgress = false;
            }
            else
            {
                if(cancellationTokenSource != null && this.cancellationTokenSource.Token.CanBeCanceled)
                    this.cancellationTokenSource.Cancel();
                IsInProgress = false;
            }

        });
        
        private CancellationTokenSource cancellationTokenSource;


        public IMvxCommand StartEditAreaCommand => new MvxCommand(async () =>
        {
            if (this.IsEditing)
                return;

            this.IsEditing = true;

            Geometry result = null;

            if (string.IsNullOrWhiteSpace(Area))
            {

                this.MapView.SketchEditor.GeometryChanged += delegate(object sender, GeometryChangedEventArgs args)
                {
                    GeometryArea = GeometryEngine.AreaGeodetic(args.NewGeometry);

                };
                result = await this.MapView.SketchEditor.StartAsync(SketchCreationMode.Polygon, true).ConfigureAwait(false);
            }
            else
            {
                var geometry = Geometry.FromJson(Area);

                await this.MapView.SetViewpointGeometryAsync(geometry, 100);



                this.MapView.SketchEditor.GeometryChanged += delegate (object sender, GeometryChangedEventArgs args)
                {
                    GeometryArea = GeometryEngine.AreaGeodetic(args.NewGeometry);

                };
                result = await this.MapView.SketchEditor.StartAsync(geometry, SketchCreationMode.Polygon).ConfigureAwait(false);
            }

            //save
            var handler = OnAreaEditCompleted;
            handler?.Invoke(new AreaEditorResult()
            {
                Geometry = result?.ToJson(),
                MapName = SelectedMap,
                Area = GeometryEngine.Area(result)
            });

            this.IsEditing = false;
            Close(this);
            //return to previous activity
        });

        private double? geometryArea;
        public double? GeometryArea
        {
            get { return this.geometryArea; }
            set { this.geometryArea = value; RaisePropertyChanged(); }
        }


        private void BtnUndo()
        {
            var command = this.MapView?.SketchEditor.UndoCommand;
            if (this.MapView?.SketchEditor?.UndoCommand.CanExecute(command) ?? false)
                this.MapView.SketchEditor.UndoCommand.Execute(command);
        }

        private void BtnDeleteCommand()
        {
            var command = this.MapView?.SketchEditor.DeleteCommand;
            if (this.MapView?.SketchEditor?.DeleteCommand.CanExecute(command) ?? false)
                this.MapView.SketchEditor.DeleteCommand.Execute(command);
        }

        public IMvxCommand UndoCommand
        {
            get
            {
                return new MvxCommand(BtnUndo);
            }
        }

        public IMvxCommand DeleteCommand
        {
            get
            {
                return new MvxCommand(BtnDeleteCommand);
            }
        }

        private bool isEditing;
        public bool IsEditing
        {
            get { return this.isEditing; }
            set { this.isEditing = value; RaisePropertyChanged(); }
        }

        
        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; RaisePropertyChanged(); }
        }
    }
}