using System;
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
        private bool IsEditing = false;

        public AreaEditorViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IMapService mapService
            
            ) 
            : base(principal, viewModelNavigationService)
        {
            this.mapService = mapService;
        }

        public override void Load()
        {
            if (!string.IsNullOrWhiteSpace(Area))
                this.geometry = Geometry.FromJson(Area);

            var mapList = mapService.GetAvailableMaps();
            var mapPath = mapList.Count != 0 ? mapList.FirstOrDefault().Value : null;

            this.mapViewModel = new MapViewModel(mapPath);

            UpdateBaseMap(mapPath);
        }

        public void UpdateBaseMap(string pathToMap)
        {
            if (pathToMap != null)
            {
                if (this.map == null)
                    this.map = new Map();

                TileCache titleCache = new TileCache(pathToMap);
                var layer = new ArcGISTiledLayer(titleCache);

                layer.MinScale = 100000000;
                layer.MaxScale = 1;

                var basemap = new Basemap(layer);
                this.map.Basemap = basemap;
            }
            else
            {
                this.map = null;
            }
        }


        public string QuestionIdentity { get; set; }

        public string Area { set; get; }

        private Geometry geometry = null;

        private Map map;
        public Map Map
        {
            get { return this.map; }
            set { this.map = value; RaisePropertyChanged(); }
        }

        private MapViewModel mapViewModel;
        public MapViewModel MapViewModel
        {
            get { return this.mapViewModel; }
            set { this.mapViewModel = value; RaisePropertyChanged(); }
        }


        MapView mapView = new MapView();


        public IMvxCommand SaveAreaCommand => new MvxCommand(() =>
        {
            var command = this.mapView.SketchEditor.CompleteCommand;
            if (this.mapView.SketchEditor.CompleteCommand.CanExecute(command))
                this.mapView.SketchEditor.CompleteCommand.Execute(command);
        });

        public IMvxCommand SwitchLocatorCommand => new MvxCommand(() =>
        {
            this.mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;
            this.mapView.LocationDisplay.IsEnabled = !this.mapView.LocationDisplay.IsEnabled;
        });


        public IMvxCommand StartEditAreaCommand => new MvxCommand(async () =>
        {
            if (this.IsEditing)
                return;

            if (this.mapView.GraphicsOverlays.Count != 0)
            {
                this.mapView.GraphicsOverlays[0].Graphics.Clear();
            }

            Geometry result = null;

            if (this.geometry == null)
                result = await this.mapView.SketchEditor.StartAsync(SketchCreationMode.Polygon, true).ConfigureAwait(false);
            else
            {
                result = await this.mapView.SketchEditor.StartAsync(this.geometry, SketchCreationMode.Polygon).ConfigureAwait(false);
            }

            //save
            var handler = OnAreaEditCompleted;
            handler?.Invoke(new AreaEditorResult()
            {
                Area = result?.ToJson()

            });

            this.IsEditing = false;

            //return to previous activity
        });

        private void BtnUndo()
        {
            var command = this.mapView.SketchEditor.UndoCommand;
            if (this.mapView.SketchEditor.UndoCommand.CanExecute(command))
                this.mapView.SketchEditor.UndoCommand.Execute(command);
        }

        public IMvxCommand UndoCommand
        {
            get
            {
                return new MvxCommand(BtnUndo);
            }
        }

        public IMvxCommand StopCommand
        {
            get
            {
                return new MvxCommand(BtnStop);
            }
        }


        private void BtnStop()
        {
            var command = this.mapView.SketchEditor.CompleteCommand;
            if (this.mapView.SketchEditor.CompleteCommand.CanExecute(command))
                this.mapView.SketchEditor.CompleteCommand.Execute(command);
        }


        public IMvxCommand NavigationCommand
        {
            get
            {
                return new MvxCommand(BtnNavigation);
            }
        }

        private void BtnNavigation()
        {
            this.mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;
            this.mapView.LocationDisplay.IsEnabled = !this.mapView.LocationDisplay.IsEnabled;
        }

        public IMvxCommand UpdateMapsCommand => new MvxCommand(async () =>
        {
            var cancel = new CancellationToken();
            await mapService.SyncMaps(cancel);
        });

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; RaisePropertyChanged(); }
        }

        public IMvxCommand CancelCommand => new MvxCommand(this.NavigateToPreviousViewModel, () => !this.IsInProgress);


        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public void NavigateToPreviousViewModel()
        {
            this.cancellationTokenSource.Cancel();
            //this.viewModelNavigationService.NavigateTo<FinishInstallationViewModel>(this.userIdentityToRelink);
        }

    }
}