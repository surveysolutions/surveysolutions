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

    readonly Intent geolocationServiceIntent = new Intent(ServiceContext, typeof(GeolocationBackgroundService));
    public event EventHandler<LocationReceivedEventArgs> LocationReceived;
    
    public GeolocationBackgroundServiceManager(IEnumeratorSettings settings)
    {
        this.settings = settings;
    }

    public bool HasGpsProvider()
    {
        var locationManager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);
        return locationManager.IsProviderEnabled(LocationManager.GpsProvider);
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
            ServiceContext.StartService(geolocationServiceIntent);
            
            serviceConnection = new ServiceConnection<GeolocationBackgroundService>();
            ServiceContext.BindService(geolocationServiceIntent, serviceConnection, Bind.AutoCreate);

            await serviceConnection.WaitOnServiceConnected();
            serviceConnection.Service.LocationReceived += ServiceOnLocationReceived;
        }

        return true;
    }

    private static Context ServiceContext => Application.Context;

    private async void ServiceOnLocationReceived(object sender, LocationReceivedEventArgs e)
    {
        var accuracyInMeters = settings.GeographyQuestionAccuracyInMeters;
        if (e.Location.Accuracy > accuracyInMeters)
            return;

        foreach (var geolocationListener in listeners.Values)
        {
            await geolocationListener.OnGpsLocationChanged(e.Location, serviceConnection.Service);
        }
        
        LocationReceived?.Invoke(sender, e);
    }

    public void StopListen(IGeolocationListener geolocationListener)
    {
        listeners.Remove(geolocationListener.GetType().Name);

        if (listeners.Count == 0 && serviceConnection != null)
        {
            serviceConnection.Service.LocationReceived -= ServiceOnLocationReceived;
            
            ServiceContext.UnbindService(serviceConnection);
            serviceConnection = null;
            
            ServiceContext.StopService(geolocationServiceIntent);
        }
    }
    
    public void Dispose()
    {
        listeners = new();
        serviceConnection?.Dispose();
        geolocationServiceIntent?.Dispose();
    }
}
