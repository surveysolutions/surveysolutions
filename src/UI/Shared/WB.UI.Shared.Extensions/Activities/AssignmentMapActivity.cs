using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.Graphics.Drawable;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.WeakSubscription;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Extensions.ViewModels;
using PopupMenu = AndroidX.AppCompat.Widget.PopupMenu;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Shared.Extensions.Activities;

[Activity(WindowSoftInputMode = SoftInput.StateHidden,
    Theme = "@style/AppTheme",
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
    Exported = false)]
public class AssignmentMapActivity : MarkersMapActivity<AssignmentMapViewModel, AssignmentMapViewModelArgs>
{
    protected override int ViewResourceId => Resource.Layout.assignment_map;
    
    protected override bool BackButtonCustomAction => true;
    protected override void BackButtonPressed()
    {
        this.ViewModel.NavigateToDashboardCommand.Execute();
        this.Finish();
    }
    
    private MvxWeakEventSubscription<ImageButton> clickedMenuButton;
    private MvxWeakEventSubscription<ImageButton> clickedZoomMenuButton;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        this.ViewModel.MapView = this.FindViewById<MapView>(Resource.Id.map_view);
        
        var buttonMenu = this.FindViewById<ImageButton>(Resource.Id.butMenu);
        clickedMenuButton = buttonMenu.WeakSubscribe(nameof(buttonMenu.Click), this.ClickedMenuButton);
        
        var buttonEye = this.FindViewById<ImageButton>(Resource.Id.butZoomMenu);
        clickedZoomMenuButton = buttonEye.WeakSubscribe(nameof(buttonEye.Click), this.ClickedZoomButton);
    }
    
    private void ClickedZoomButton(object sender, EventArgs e)
    {
        ShowPopupMenu((ImageButton)sender, [
            new CustomMenuItem(UIResources.MenuItem_Title_ZoomMap, () => this.ViewModel.ShowFullMapCommand.Execute(), Resource.Drawable.icon_zoom_map),
            new CustomMenuItem(UIResources.MenuItem_Title_ZoomShapefile, () => this.ViewModel.ShowShapefileCommand.Execute(), Resource.Drawable.icon_zoom_everything),
            new CustomMenuItem(UIResources.MenuItem_Title_ZoomCollectedData, () => this.ViewModel.ShowAllItemsCommand.Execute(), Resource.Drawable.icon_zoom_collected_data),
            new CustomMenuItem(UIResources.MenuItem_Title_ZoomGeoTracking, () => this.ViewModel.ShowTrackingDataCommand.Execute(), Resource.Drawable.icon_zoom_geotracking_data)
        ]);
    }

    private void ClickedMenuButton(object sender, EventArgs e)
    {
        List<CustomMenuItem> customMenuItems = new();
        customMenuItems.Add(new CustomMenuItem(UIResources.MenuItem_Title_CreateInterview, () => this.ViewModel.CreateInterviewCommand.Execute(), Resource.Drawable.icon_create_interview_menu));
        customMenuItems.Add(new CustomMenuItem(UIResources.MenuItem_Title_ChangeMap, () => this.ViewModel.SwitchMapCommand.Execute(), Resource.Drawable.icon_change_map));
            //new CustomMenuItem("Change Boundaries", () => this.ViewModel.LoadShapefile.Execute(), Resource.Drawable.icon_change_map),
            
        if (ViewModel.IsGeoTrackingPemitted)
            customMenuItems.Add(this.ViewModel.IsEnabledGeoTracking
                ? new CustomMenuItem(UIResources.MenuItem_Title_StopGeoTracking, () => this.ViewModel.StartGeoTrackingCommand.Execute(), Resource.Drawable.icon_geotracking_default, ViewModel.IsGeoTrackingAvailable)
                : new CustomMenuItem(UIResources.MenuItem_Title_StartGeoTracking, () => this.ViewModel.StartGeoTrackingCommand.Execute(), Resource.Drawable.icon_geotracking_default, ViewModel.IsGeoTrackingAvailable));

        if (ViewModel.IsGeofencingPermitted)
            customMenuItems.Add(this.ViewModel.IsEnabledGeofencing
                ? new CustomMenuItem(UIResources.MenuItem_Title_StopGeofencing, () => this.ViewModel.StartGeofencingCommand.Execute(), Resource.Drawable.icon_geofencing_default, ViewModel.IsGeofencingAvailable)
                : new CustomMenuItem(UIResources.MenuItem_Title_StartGeofencing, () => this.ViewModel.StartGeofencingCommand.Execute(), Resource.Drawable.icon_geofencing_default, ViewModel.IsGeofencingAvailable));

        customMenuItems.Add(new CustomMenuItem(UIResources.MenuItem_Title_ExitToDashboard, () => this.ViewModel.NavigateToDashboardCommand.Execute(), Resource.Drawable.icon_exit));

        ShowPopupMenu((ImageButton)sender, customMenuItems);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            clickedMenuButton?.Dispose();
            clickedMenuButton = null;
            
            clickedZoomMenuButton?.Dispose();
            clickedZoomMenuButton = null;
        }
        
        base.Dispose(disposing);
    }
}
