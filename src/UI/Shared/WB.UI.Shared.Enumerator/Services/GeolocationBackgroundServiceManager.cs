using Android.Content;
using Android.Locations;
using Android.OS;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Shared.Enumerator.Services;

[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
public class GeolocationBackgroundServiceManager : IGeolocationBackgroundServiceManager, IDisposable
{
    private readonly IEnumeratorSettings settings;
    private Dictionary<string, IGeolocationListener> listeners = new();
    private ServiceConnection<GeolocationBackgroundService> serviceConnection;

    private Intent GetGeolocationServiceIntent() => new Intent(ServiceContext, typeof(GeolocationBackgroundService));
    public event EventHandler<LocationReceivedEventArgs> LocationReceived;
    
    public GeolocationBackgroundServiceManager(IEnumeratorSettings settings)
    {
        this.settings = settings;
    }

    public bool HasGpsProvider()
    {
        var locationManager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);
        if (locationManager == null) return false;

        // On API 28+, location is a single on/off toggle. IsLocationEnabled is the
        // correct check — IsProviderEnabled("gps") only reflects hardware GPS state and
        // returns false when hardware GPS is off but an external sensor (mock location app)
        // is actively injecting fixes.
        if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            return locationManager.IsLocationEnabled;

        // API < 28: check GPS provider specifically, or any enabled provider as fallback.
        if (locationManager.IsProviderEnabled(LocationManager.GpsProvider))
            return true;
        try
        {
            return locationManager.GetProviders(enabledOnly: true).Count > 0;
        }
        catch
        {
            return false;
        }
    }

    public IGeolocationListener GetListen(IGeolocationListener geolocationListener)
    {
        return listeners.GetValueOrDefault(geolocationListener.GetType().Name);
    }

    public async Task<bool> StartListen(IGeolocationListener geolocationListener)
    {
        if (!HasGpsProvider())
            return false;
        
        listeners[geolocationListener.GetType().Name] = geolocationListener;

        if (listeners.Count > 0 && serviceConnection == null)
        {
            ServiceContext.StartService(GetGeolocationServiceIntent());
            
            serviceConnection = new ServiceConnection<GeolocationBackgroundService>();
            ServiceContext.BindService(GetGeolocationServiceIntent(), serviceConnection, Bind.AutoCreate);

            await serviceConnection.WaitOnServiceConnected();
            serviceConnection.Service.LocationReceived += ServiceOnLocationReceived;
        }

        return true;
    }

    private static Context ServiceContext => Application.Context;

    private async void ServiceOnLocationReceived(object sender, LocationReceivedEventArgs e)
    {
        // Skip the accuracy filter for external GPS sensors (mock provider): they often
        // report a fixed or vendor-specific accuracy value that does not reflect actual
        // signal quality. Applying the threshold would silently drop every fix until timeout.
        if (!e.IsFromMockProvider)
        {
            var accuracyInMeters = settings.GeographyQuestionAccuracyInMeters;
            if (e.Location.Accuracy > accuracyInMeters)
                return;
        }

        // Create a snapshot to avoid collection modification during enumeration
        var listenersCopy = listeners.Values.ToArray();
        foreach (var geolocationListener in listenersCopy)
        {
            await geolocationListener.OnGpsLocationChanged(e.Location, serviceConnection.Service);
        }
        
        LocationReceived?.Invoke(sender, e);
    }

    public void StopListen(IGeolocationListener geolocationListener)
    {
        listeners.Remove(geolocationListener.GetType().Name);

        if (listeners.Count == 0)
            UnbindService();
    }

    private bool UnbindService()
    {
        if (serviceConnection != null)
        {
            serviceConnection.Service.LocationReceived -= ServiceOnLocationReceived;
                
            ServiceContext.UnbindService(serviceConnection);
            serviceConnection = null;
                
            return ServiceContext.StopService(GetGeolocationServiceIntent());
        }
        return true;
    }

    public bool StopAll()
    {
        listeners.Clear();
        return UnbindService();
    }
    public void Dispose()
    {
        UnbindService();
        
        listeners = new();
        serviceConnection?.Dispose();
        //geolocationServiceIntent?.Dispose();
    }
}
