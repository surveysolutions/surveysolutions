using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.Views;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Infrastructure.Shared.Enumerator.Internals.MapService;

namespace WB.UI.Shared.Extensions.CustomServices.AreaEditor
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
            this.AvailableMaps = this.mapService.GetAvailableMaps();
            this.MapsList = this.AvailableMaps.Keys.ToList();

            if (this.AvailableMaps.Count == 0) return;

            if (!string.IsNullOrEmpty(this.MapName) && this.AvailableMaps.ContainsKey(this.MapName))
            {
                this.SelectedMap = this.MapName;
            }
            else
            {
                this.SelectedMap = this.AvailableMaps.FirstOrDefault().Key;
            }
        }

        private Dictionary<string, string> AvailableMaps = new Dictionary<string, string>();

        private List<string> mapsList;

        public List<string> MapsList
        {
            get { return this.mapsList; }
            set
            {
                this.mapsList = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => SelectedMap); //fix
            }
        }

        private string selectedMap;

        public string SelectedMap
        {
            get { return this.selectedMap; }
            set
            {
                this.selectedMap = value;
                RaisePropertyChanged();

                if (this.AvailableMaps.ContainsKey(value))
                {
                    this.UpdateBaseMap(this.AvailableMaps[value]);
                }

            }
        }

        public void UpdateBaseMap(string pathToMap)
        {
            if (pathToMap != null)
            {
                try
                {
                    if(MapView?.LocationDisplay != null)
                        this.MapView.LocationDisplay.IsEnabled = false;
                }
                catch (Exception exc)
                {
                    
                    //throw;
                }
                


                var map = new Map();

                TileCache titleCache = new TileCache(pathToMap);
                var layer = new ArcGISTiledLayer(titleCache);

                //zoom to any level
                //if area is out of the map
                // should be available to navigate
                layer.MinScale = 100000000;
                layer.MaxScale = 1;
                //

                map.Basemap = new Basemap(layer);
                
                map.MinScale = 100000000;
                map.MaxScale = 1;

                this.Map = map;
            }
            else
            {
                this.Map = null;
            }
        }
        
        public void Init(string geometry, string mapName, double? areaSize)
        {
            this.Area = geometry;
            this.MapName = mapName;
            this.GeometryArea = areaSize;
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


                if (this.mapView != null)
                {
                    this.mapView.ViewAttachedToWindow +=
                        delegate(object sender, View.ViewAttachedToWindowEventArgs args)
                        {
                            if (!string.IsNullOrEmpty(this.Area))
                                if (this.StartEditAreaCommand.CanExecute())
                                    this.StartEditAreaCommand.Execute();
                        };
                }
            }
            get { return this.mapView; }
        }

        public IMvxCommand SaveAreaCommand => new MvxCommand(() =>
        {
            var command = this.MapView.SketchEditor.CompleteCommand;
            if (this.MapView.SketchEditor.CompleteCommand.CanExecute(command))
            {
                
                /*if(!GeometryEngine.IsSimple(this.MapView.SketchEditor.Geometry))
                {
                    userInteractionService.ShowToast("Area is invalid. Please fix.");
                    return;
                }
                else*/
                    this.MapView.SketchEditor.CompleteCommand.Execute(command);
            }
            
            else
            {
                this.userInteractionService.ShowToast("No changes we made to be saved");
            }
        });

        public IMvxCommand SwitchLocatorCommand => new MvxCommand(() =>
        {
            if(!this.MapView.LocationDisplay.IsEnabled)
                this.MapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;

            this.MapView.LocationDisplay.IsEnabled = !this.MapView.LocationDisplay.IsEnabled;
        });

        public IMvxCommand UpdateMapsCommand => new MvxCommand(async () =>
        {
            if (!this.IsInProgress)
            {
                this.IsInProgress = true;
                this.cancellationTokenSource = new CancellationTokenSource();
                await this.mapService.SyncMaps(this.cancellationTokenSource.Token);

                this.AvailableMaps = this.mapService.GetAvailableMaps();
                this.MapsList = this.AvailableMaps.Keys.ToList();

                this.IsInProgress = false;
            }
            else
            {
                if(this.cancellationTokenSource != null && this.cancellationTokenSource.Token.CanBeCanceled)
                    this.cancellationTokenSource.Cancel();
                this.IsInProgress = false;
            }

        });
        
        private CancellationTokenSource cancellationTokenSource;


        public IMvxCommand StartEditAreaCommand => new MvxCommand(async () =>
        {
            if (this.IsEditing)
                return;

            this.IsEditing = true;
            this.MapView.SketchEditor.GeometryChanged += delegate (object sender, GeometryChangedEventArgs args)
            {
                this.GeometryArea = GeometryEngine.AreaGeodetic(args.NewGeometry);
                this.CanUndo = this.MapView.SketchEditor.UndoCommand.CanExecute(this.MapView.SketchEditor.UndoCommand);
                this.CanSave = this.MapView.SketchEditor.CompleteCommand.CanExecute(this.MapView.SketchEditor.CompleteCommand);
            };

            Geometry result = null;
            if (string.IsNullOrWhiteSpace(this.Area))
            {
                result = await this.MapView.SketchEditor.StartAsync(SketchCreationMode.Polygon, true).ConfigureAwait(false);
            }
            else
            {
                var geometry = Geometry.FromJson(this.Area);
                await this.MapView.SetViewpointGeometryAsync(geometry, 120);
                result = await this.MapView.SketchEditor.StartAsync(geometry, SketchCreationMode.Polygon).ConfigureAwait(false);
            }

            //save
            var handler = this.OnAreaEditCompleted;
            var position = this.MapView.LocationDisplay.Location.Position;

            double? dist = null;
            if (position != null)
            {
                var point = new MapPoint(position.X, position.Y, position.Z, this.MapView.SpatialReference);
                dist = GeometryEngine.Distance(result, point);
            }

            var resultArea = new AreaEditorResult()
            {
                Geometry = result?.ToJson(),
                MapName = this.SelectedMap,
                Area = GeometryEngine.AreaGeodetic(result),
                Length = GeometryEngine.LengthGeodetic(result),
                DistanceToEditor = dist
            };
            handler?.Invoke(resultArea);

            this.IsEditing = false;
            Close(this);
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

        public IMvxCommand UndoCommand => new MvxCommand(this.BtnUndo);
        public IMvxCommand DeleteCommand => new MvxCommand(this.BtnDeleteCommand);

        private bool isEditing;
        public bool IsEditing
        {
            get { return this.isEditing; }
            set { this.isEditing = value; RaisePropertyChanged(); }
        }


        private bool canUndo;
        public bool CanUndo
        {
            get { return this.canUndo; }
            set { this.canUndo = value; RaisePropertyChanged(); }
        }

        private bool canSave;
        public bool CanSave
        {
            get { return this.canSave; }
            set { this.canSave = value; RaisePropertyChanged(); }
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; RaisePropertyChanged(); }
        }
    }
}