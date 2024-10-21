using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Esri.ArcGISRuntime.UI.Controls;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Extensions.ViewModels;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Shared.Extensions.Activities;

[Activity(WindowSoftInputMode = SoftInput.StateHidden,
    Theme = "@style/AppTheme",
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
    Exported = false)]
public class GeofencingActivity : BaseActivity<GeofencingViewModel>
{
    protected override int ViewResourceId => Resource.Layout.geofencing;
    
    GeolocationBackgroundService locationService;
    bool isBound = false;

    private ServiceConnection serviceConnection = null;

    public GeofencingActivity()
    {
        serviceConnection = new ServiceConnection(this);
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        this.ViewModel.MapView = this.FindViewById<MapView>(Resource.Id.map_view);

        var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
        toolbar.Title = "";
        this.SetSupportActionBar(toolbar);

        
        var intent = new Intent(this, typeof(GeolocationBackgroundService));
        StartService(intent);
        BindService(intent, serviceConnection, Bind.AutoCreate);
    }

    private class ServiceConnection : Java.Lang.Object, IServiceConnection
    {
        private GeofencingActivity activity;

        public ServiceConnection(GeofencingActivity activity)
        {
            this.activity = activity;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var binder = (ServiceBinder<GeolocationBackgroundService>)service;
            activity.locationService = binder.GetService(); 
            activity.isBound = true;

            activity.locationService.LocationReceived += activity.LocationServiceOnLocationReceived;
        }


        public void OnServiceDisconnected(ComponentName name)
        {
            activity.isBound = false;
        }
    }
    
    private async void LocationServiceOnLocationReceived(object sender, LocationReceivedEventArgs args)
    {
        var result = await ViewModel.CheckIfInsideOfShapefile(args.Location);
    }

    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (isBound)
        {
            locationService.LocationReceived -= LocationServiceOnLocationReceived;
            UnbindService(serviceConnection);
            isBound = false;
        }
    }
}
