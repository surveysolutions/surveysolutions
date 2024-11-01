using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Esri.ArcGISRuntime.UI.Controls;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Extensions.ViewModels;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Shared.Extensions.Activities;

[Activity(WindowSoftInputMode = SoftInput.StateHidden,
    Theme = "@style/AppTheme",
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
    Exported = false)]
public class GeofencingActivity : MarkersMapActivity<GeofencingViewModel, GeofencingViewModelArgs>
{
    protected override int ViewResourceId => Resource.Layout.geofencing;
    
    protected override bool BackButtonCustomAction => true;
    protected override void BackButtonPressed()
    {
        this.ViewModel.NavigateToDashboardCommand.Execute();
        this.Finish();
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        this.ViewModel.MapView = this.FindViewById<MapView>(Resource.Id.map_view);

        var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
        toolbar.Title = "";
        this.SetSupportActionBar(toolbar);
    }
    

    public override bool OnCreateOptionsMenu(IMenu menu)
    {
        this.MenuInflater.Inflate(Resource.Menu.geofencing, menu);

        menu.LocalizeMenuItem(Resource.Id.menu_geofencing, UIResources.MenuItem_Title_Geofencing);
        menu.LocalizeMenuItem(Resource.Id.menu_geo_tracking, UIResources.MenuItem_Title_GeoTracking);

        return base.OnCreateOptionsMenu(menu);
    }
    
    public override bool OnOptionsItemSelected(IMenuItem item)
    {
        if(item.ItemId == Resource.Id.menu_geofencing)
            this.ViewModel.StartGeofencingCommand.Execute();
        if(item.ItemId == Resource.Id.menu_geo_tracking)
            this.ViewModel.StartGeoTrackingCommand.Execute();
            
        return base.OnOptionsItemSelected(item);
    }
}
