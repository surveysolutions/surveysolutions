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
        public static event Action<AreaEditorResult> OnAreaEditCompleted;

        MapViewModel mapViewModel = new MapViewModel();
        MapView mapView = new MapView();

        private Geometry geometry = null;
        private bool IsEditing = false;

        public override void OnBackPressed()
        {
            this.Cancel();
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
            this.mapView.LocationDisplay.IsEnabled = !this.mapView.LocationDisplay.IsEnabled;
        }

        //issue in esri . inflating from layout doesn't work
        private void CreateLayout()
        {
            // Create Button that will start the Feature Query
            var startButton = new Button(this);
            startButton.Text = "Start Edit";
            startButton.Click += BtnClickAsync;
            
            var ButtonUndo = new Button(this);
            ButtonUndo.Text = "Undo";
            ButtonUndo.Click += BtnUndo;

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
            startLocatorButton.Text = "Locate";
            startLocatorButton.Click += OnStartButtonClicked;

            LinearLayout topContainer = new LinearLayout(this);

            topContainer.AddView(startButton);
            topContainer.AddView(ButtonUndo);
            //topContainer.AddView(startLocatorButton);

            LinearLayout bottomContainer = new LinearLayout(this);

            bottomContainer.AddView(ButtonSave);
            bottomContainer.AddView(ButtonCancel);

            // Add the map view to the layout
            var layoutParamsMain = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, 
                LinearLayout.LayoutParams.MatchParent);
            
            var layoutParamsButtons = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, 
                LinearLayout.LayoutParams.WrapContent);

            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Android.Widget.Orientation.Vertical };
            layout.AddView(topContainer, layoutParamsButtons);
            layout.AddView(bottomContainer, layoutParamsButtons);

            var relative = new RelativeLayout(this);
            relative.AddView(mapView);
            relative.AddView(startLocatorButton);

            layout.AddView(relative, layoutParamsMain);
            
            // Show the layout in the app
            SetContentView(layout);
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
    }
}