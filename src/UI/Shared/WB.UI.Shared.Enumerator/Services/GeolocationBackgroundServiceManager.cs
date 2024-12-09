using Android.Content;
using Android.Locations;
using Android.OS;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Shared.Enumerator.Services;

[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
public class GeolocationBackgroundServiceManager : IGeolocationBackgroundServiceManager, IDisposable
{
    private HashSet<IGeolocationListener> listeners = new HashSet<IGeolocationListener>();
    private ServiceConnection<GeolocationBackgroundService> serviceConnection;

    readonly Intent geolocationServiceIntent = new Intent(Application.Context, typeof(GeolocationBackgroundService));
    public event EventHandler<LocationReceivedEventArgs> LocationReceived;
    
    public GeolocationBackgroundServiceManager()
    {
    }

    public bool HasGpsProvider()
    {
        var locationManager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);
        return locationManager.IsProviderEnabled(LocationManager.GpsProvider);
    }

    public async Task<bool> StartListen(IGeolocationListener geolocationListener)
    {
        if (!HasGpsProvider())
            return false;
        
        listeners.Add(geolocationListener);

        if (listeners.Count > 0 && serviceConnection == null)
        {
            Application.Context.StartService(geolocationServiceIntent);
            
            serviceConnection = new ServiceConnection<GeolocationBackgroundService>();
            Application.Context.BindService(geolocationServiceIntent, serviceConnection, Bind.AutoCreate);

            await serviceConnection.WaitOnServiceConnected();
            serviceConnection.Service.LocationReceived += ServiceOnLocationReceived;
        }

        return true;
    }

    private async void ServiceOnLocationReceived(object sender, LocationReceivedEventArgs e)
    {
        foreach (var geolocationListener in listeners)
        {
            await geolocationListener.OnGpsLocationChanged(e.Location, serviceConnection.Service);
        }
        
        LocationReceived?.Invoke(sender, e);
    }

    public void StopListen(IGeolocationListener geolocationListener)
    {
        listeners.Remove(geolocationListener);

        if (listeners.Count == 0 && serviceConnection != null)
        {
            serviceConnection.Service.LocationReceived -= ServiceOnLocationReceived;
            
            Application.Context.UnbindService(serviceConnection);
            serviceConnection = null;
            
            Application.Context.StopService(geolocationServiceIntent);
        }
    }
    
    public void Dispose()
    {
        listeners = new HashSet<IGeolocationListener>();
        serviceConnection?.Dispose();
        geolocationServiceIntent?.Dispose();
    }
}
