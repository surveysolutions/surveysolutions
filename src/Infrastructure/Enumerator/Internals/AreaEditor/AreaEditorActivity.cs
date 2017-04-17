using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;

namespace WB.Infrastructure.Shared.Enumerator.Internals.AreaEditor
{
    [Activity(ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation, Label = "AreaEditorActivity")]
    public class AreaEditorActivity : Activity
    {
        public static event System.Action<AreaEditorResult> OnAreaEditCompleted;

        MapViewModel mapViewModel = new MapViewModel();
        MapView mapView = new MapView();

        private Geometry geometry = null;
        private bool IsEditing = false;

        public override void OnBackPressed()
        {
            AreaEditorActivity.OnAreaEditCompleted((AreaEditorResult)null);
            this.Finish();
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            string Area = Intent.GetStringExtra(Android.Content.Intent.ExtraText);

            if (!string.IsNullOrWhiteSpace(Area))
                this.geometry = Geometry.FromJson(Area);

            CreateLayout();
            if (mapViewModel.Map == null)
            {
                Toast.MakeText(this, "Map was not found", ToastLength.Long);
            }

            mapView.Map = mapViewModel.Map;
            mapViewModel.PropertyChanged += MapViewModel_PropertyChanged;

            if (this.geometry != null)
            {
                if(this.mapView.GraphicsOverlays.Count == 0)
                    this.mapView.GraphicsOverlays.Add(new GraphicsOverlay());

                this.mapView.GraphicsOverlays[0].Graphics.Add(new Graphic() {Geometry = this.geometry});
            }
        }

        private void MapViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Map" && mapView != null)
            {
                mapView.Map = mapViewModel.Map;
            }

        }

        private async void BtnClickAsync(object sender, EventArgs e)
        {
            if (IsEditing)
                return;

            await this.StartEditor();
        }


        private async Task StartEditor()
        {
            IsEditing = true;

            if (this.mapView.GraphicsOverlays.Count != 0)
            {
                this.mapView.GraphicsOverlays[0].Graphics.Clear();
            }

            Geometry result = null;

            if (geometry == null)
                result = await mapView.SketchEditor.StartAsync(SketchCreationMode.Polygon, true);
            else
            {
                result = await mapView.SketchEditor.StartAsync(geometry, SketchCreationMode.Polygon);
            }

            AreaEditorActivity.OnAreaEditCompleted(new AreaEditorResult()
            {
                Area = result?.ToJson()
            });

            IsEditing = false;

            this.Finish();
        }

        private void BtnUndo(object sender, EventArgs e)
        {
            var command = mapView.SketchEditor.UndoCommand;
            if (mapView.SketchEditor.UndoCommand.CanExecute(command))
                mapView.SketchEditor.UndoCommand.Execute(command);
        }

        private void BtnRedo(object sender, EventArgs e)
        {
            var command = mapView.SketchEditor.RedoCommand;
            if (mapView.SketchEditor.RedoCommand.CanExecute(command))
                mapView.SketchEditor.RedoCommand.Execute(command);
        }

        private void BtnClear(object sender, EventArgs e)
        {
            mapView.SketchEditor.ClearGeometry();
        }

        private void BtnStopClick(object sender, EventArgs e)
        {
            var command = mapView.SketchEditor.CompleteCommand;
            if (mapView.SketchEditor.CompleteCommand.CanExecute(command))
                mapView.SketchEditor.CompleteCommand.Execute(command);
        }

        private void OnShowButtonClicked(object sender, EventArgs e)
        {
            Toast.MakeText(this, geometry?.ToJson(), ToastLength.Long).Show();
        }

        private void OnStartButtonClicked(object sender, EventArgs e)
        {
            mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;

            //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
            if (!mapView.LocationDisplay.IsEnabled)
                mapView.LocationDisplay.IsEnabled = true;
            else
            {
                mapView.LocationDisplay.IsEnabled = false;
            }
        }

        //issue in esri . inflating from layout doesn't work
        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Android.Widget.Orientation.Vertical };

            // Create Button that will start the Feature Query
            var queryButton = new Button(this);
            queryButton.Text = "Start Edit";
            queryButton.Click += BtnClickAsync;

            var ButtonUndo = new Button(this);
            ButtonUndo.Text = "Undo";
            ButtonUndo.Click += BtnUndo;

            var ButtonRedo = new Button(this);
            ButtonRedo.Text = "Redo";
            ButtonRedo.Click += BtnRedo;

            var ButtonClear = new Button(this);
            ButtonClear.Text = "Clear";
            ButtonClear.Click += BtnClear;

            var ButtonSave = new Button(this);
            ButtonSave.Text = "Save";
            ButtonSave.Click += BtnStopClick;


            var ButtonCancel = new Button(this);
            ButtonCancel.Text = "Cancel";
            ButtonCancel.Click += BtnCancelClick;

            var showButton = new Button(this);
            showButton.Text = "Show";
            showButton.Click += OnShowButtonClicked;


            var startLocatorButton = new Button(this);
            startLocatorButton.Text = "Locator";
            startLocatorButton.Click += OnStartButtonClicked;

            LinearLayout topContainer = new LinearLayout(this);

            topContainer.AddView(queryButton);
            topContainer.AddView(ButtonUndo);
            topContainer.AddView(ButtonRedo);
            topContainer.AddView(ButtonClear);

            LinearLayout locationContainer = new LinearLayout(this);
            locationContainer.AddView(startLocatorButton);
            
            LinearLayout bottomContainer = new LinearLayout(this);

            bottomContainer.AddView(ButtonSave);
            bottomContainer.AddView(ButtonCancel);

            // Add the map view to the layout
            var layout_width = LinearLayout.LayoutParams.MatchParent;
            var layout_height = LinearLayout.LayoutParams.MatchParent;
            LinearLayout.LayoutParams layoutParamsMain = new LinearLayout.LayoutParams(layout_width, layout_height);
            
            LinearLayout.LayoutParams layoutParamsButtons = 
                new LinearLayout.LayoutParams(layout_width, LinearLayout.LayoutParams.WrapContent);

            layout.AddView(topContainer, layoutParamsButtons);
            layout.AddView(locationContainer, layoutParamsButtons);
            layout.AddView(bottomContainer, layoutParamsButtons);

            layout.AddView(mapView, layoutParamsMain);

            // Show the layout in the app
            SetContentView(layout);
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            AreaEditorActivity.OnAreaEditCompleted((AreaEditorResult)null);
            //IsEedit = false;
            this.Finish();
        }
    }
}