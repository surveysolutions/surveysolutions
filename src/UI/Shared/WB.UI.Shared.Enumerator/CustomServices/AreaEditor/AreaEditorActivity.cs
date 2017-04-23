using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.Platform;

namespace WB.UI.Shared.Enumerator.CustomServices.AreaEditor
{
    //[Activity(WindowSoftInputMode = SoftInput.StateHidden, Theme = "@style/GrayAppTheme", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    //public class AreaEditorActivity : Activity//BaseActivity<AreaEditorViewModel>

    [Activity(ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation, Label = "AreaEditorActivity")]
    public class AreaEditorActivity : Activity
    {
        public static event Action<AreaEditorResult> OnAreaEditCompleted;

        MapViewModel mapViewModel ;
        MapView mapView = new MapView();

        private IMapService mapService;

        private Geometry geometry = null;
        private bool IsEditing = false;

        public override void OnBackPressed()
        {
            this.Cancel();
        }

        //protected override int ViewResourceId => Resource.Layout.interview_area_editor;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            mapService = Mvx.Resolve<IMapService>();

            var mapList = mapService.GetAvailableMaps();

            var mapPath = mapList.Count != 0 ? mapList.FirstOrDefault().Value : null;

            this.mapViewModel = new MapViewModel(mapPath);
            
            string Area = Intent.GetStringExtra(Android.Content.Intent.ExtraText);//this.ViewModel.Area;

            if (!string.IsNullOrWhiteSpace(Area))
                this.geometry = Geometry.FromJson(Area);

            this.CreateLayout();
            if (this.mapViewModel.Map == null)
            {
                Toast.MakeText(this, "Map was not found", ToastLength.Long);
                //this.Finish();
            }

            this.mapView.Map = this.mapViewModel.Map;
            
            this.mapViewModel.PropertyChanged += this.MapViewModel_PropertyChanged;

        }

        private void MapViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Map" && this.mapView != null)
            {
                this.mapView.Map = this.mapViewModel.Map;
            }

        }

        private async void BtnClickAsync(object sender, EventArgs e)
        {
            if (this.IsEditing)
                return;

            await this.StartEditor();
        }


        private async Task StartEditor()
        {
            this.IsEditing = true;

            if (this.mapView.GraphicsOverlays.Count != 0)
            {
                this.mapView.GraphicsOverlays[0].Graphics.Clear();
            }

            Geometry result = null;

            if (this.geometry == null)
                result = await this.mapView.SketchEditor.StartAsync(SketchCreationMode.Polygon, true);
            else
            {
                result = await this.mapView.SketchEditor.StartAsync(this.geometry, SketchCreationMode.Polygon);
            }
            
            AreaEditorActivity.OnAreaEditCompleted(new AreaEditorResult()
            {
                Area = result?.ToJson()
                
            });

            this.IsEditing = false;

            this.Finish();
        }

        private void OnMapButtonClicked(object sender, EventArgs e)
        {
            var startButton = sender as Button;

            // Create menu to show navigation options
            var navigationMenu = new PopupMenu(this, startButton);
            navigationMenu.MenuItemClick += OnMapSelected;

            // Create menu options
            foreach (var map in mapService.GetAvailableMaps().Keys)
                navigationMenu.Menu.Add(map);

            // Show menu in the view
            navigationMenu.Show();
        }

        private void OnMapSelected(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            var selectedNavigationType = e.Item.TitleCondensedFormatted.ToString();
            var maps = mapService.GetAvailableMaps();
            if (maps.ContainsKey(selectedNavigationType))
                this.mapViewModel = new MapViewModel(maps[selectedNavigationType]);

        }

        private void BtnUndo(object sender, EventArgs e)
        {
            var command = this.mapView.SketchEditor.UndoCommand;
            if (this.mapView.SketchEditor.UndoCommand.CanExecute(command))
                this.mapView.SketchEditor.UndoCommand.Execute(command);
        }

        private void BtnStopClick(object sender, EventArgs e)
        {
            var command = this.mapView.SketchEditor.CompleteCommand;
            if (this.mapView.SketchEditor.CompleteCommand.CanExecute(command))
                this.mapView.SketchEditor.CompleteCommand.Execute(command);
        }

        private void OnShowButtonClicked(object sender, EventArgs e)
        {
            Toast.MakeText(this, this.geometry?.ToJson(), ToastLength.Long).Show();
        }

        private void OnStartButtonClicked(object sender, EventArgs e)
        {
            this.mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;
            this.mapView.LocationDisplay.IsEnabled = !this.mapView.LocationDisplay.IsEnabled;
        }

        //issue in esri . inflating from layout doesn't work
        private void CreateLayout()
        {

            var mapSelectorButton = new Button(this);
            mapSelectorButton.Text = "Select Map";
            mapSelectorButton.Click += this.OnMapButtonClicked;

            var startButton = new Button(this);
            startButton.Text = "Start Edit";
            startButton.Click += this.BtnClickAsync;
            startButton.Enabled = this.mapViewModel.Map != null;


            var ButtonUndo = new Button(this);
            ButtonUndo.Text = "Undo";
            ButtonUndo.Click += this.BtnUndo;
            ButtonUndo.Enabled = this.mapViewModel.Map != null;

            var ButtonSave = new Button(this);
            ButtonSave.Text = "Save";
            ButtonSave.Click += this.BtnStopClick;
            ButtonSave.Enabled = this.mapViewModel.Map != null;

            var ButtonCancel = new Button(this);
            ButtonCancel.Text = "Cancel";
            ButtonCancel.Click += this.BtnCancelClick;

            var showButton = new Button(this);
            showButton.Text = "Show";
            showButton.Click += this.OnShowButtonClicked;

            var ButtonUpdateMaps = new Button(this);
            ButtonUpdateMaps.Text = "Update maps";
            ButtonUpdateMaps.Click += this.ButtonUpdateMaps;

            var startLocatorButton = new Button(this);
            startLocatorButton.Text = "Position";
            startLocatorButton.Click += this.OnStartButtonClicked;
            startLocatorButton.Enabled = this.mapViewModel.Map != null;

            var lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent)
                { Gravity = GravityFlags.Right};

            LinearLayout topContainer = new LinearLayout(this);

            topContainer.AddView(mapSelectorButton);
            topContainer.AddView(ButtonUpdateMaps, lp);


            LinearLayout middleContainer = new LinearLayout(this);
            middleContainer.AddView(startButton);
            middleContainer.AddView(ButtonUndo);
            
            LinearLayout bottomContainer = new LinearLayout(this);

            bottomContainer.AddView(ButtonSave);
            bottomContainer.AddView(ButtonCancel);
            
            var layoutParamsButtons = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, 
                LinearLayout.LayoutParams.WrapContent);
            
            
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
            //layout.AddView(topContainer, layoutParamsButtons);

            layout.AddView(middleContainer, layoutParamsButtons);
            layout.AddView(bottomContainer, layoutParamsButtons);
            layout.AddView(startLocatorButton);
            
            var relative = new RelativeLayout(this);
            relative.AddView(this.mapView);
            
            relative.AddView(layout);


            var layoutMain = new LinearLayout(this) { Orientation = Orientation.Vertical };

            layoutMain.AddView(topContainer);
            layoutMain.AddView(relative);
            // Show the layout in the app

            this.SetContentView(layoutMain);
        }

        private async void ButtonUpdateMaps(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
                btn.Text = "Checking..";

            var cancel = new CancellationToken();
            await mapService.SyncMaps(cancel);

            if (btn != null)
                btn.Text = "Update maps";
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            this.Cancel();
        }

        private void Cancel()
        {
            AreaEditorActivity.OnAreaEditCompleted((AreaEditorResult)null);
            this.Finish();
        }


        /*
         public override void OnBackPressed()
                {
                    this.ViewModel.NavigateToPreviousViewModel(() =>
                    {
                        this.ViewModel.NavigateBack();
                        this.Finish();
                    });
                }

        */



        /*

            string pathToSearch = "TheWorldBank/shared/maps/";
            //string pathToSearchDefaultMap = "TheWorldBank/shared/maps/base";
            string tpkToSearch = "*.tpk";
            
            
            string searchPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, pathToSearch);
            //string searchDefaultPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, pathToSearchDefaultMap);

            var tpkFileSearchResult = Directory.GetFiles(searchPath, tpkToSearch);
            //var tpkDefaultFileSearchResult = Directory.GetFiles(searchDefaultPath, tpkToSearch);

            var map = new Map();

            /*var serviceUri = new Uri("http://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer");
            map.Basemap.BaseLayers.Add(new ArcGISTiledLayer(serviceUri));#1#

            var la = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, pathToSearch);

            var vectorLayer = new ArcGISVectorTiledLayer(new Uri(Directory.GetFiles(la, "*.vtpk").First()));
            //map.Basemap.BaseLayers.Add(vectorLayer);

*/
    }
}