using Android.Content;
using Android.OS;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Shared.Enumerator.Services;

[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
public class GeolocationBackgroundServiceManager : GeolocationBackgroundService, IGeolocationBackgroundServiceManager
{
    private static HashSet<IGeolocationListener> listeners = new HashSet<IGeolocationListener>();
    private static bool serviceIsRunning = false;

    //readonly Intent geolocationServiceIntent = new Intent(this, typeof(GeolocationBackgroundServiceManager));

    public GeolocationBackgroundServiceManager()
    {
    }

    public void StartListen(IGeolocationListener geolocationListener)
    {
        listeners.Add(geolocationListener);

        if (listeners.Count > 0 && !serviceIsRunning)
        {
            serviceIsRunning = true;

            Intent geolocationServiceIntent = new Intent(Application.Context, typeof(GeolocationBackgroundServiceManager));
            Application.Context.StartService(geolocationServiceIntent);
        }
    }
    
    public void StopListen(IGeolocationListener geolocationListener)
    {
        listeners.Remove(geolocationListener);

        if (listeners.Count == 0 && serviceIsRunning)
        {
            serviceIsRunning = false;

            Intent geolocationServiceIntent = new Intent(Application.Context, typeof(GeolocationBackgroundServiceManager));
            Application.Context.StopService(geolocationServiceIntent);
        }
    }
    
    protected override async void OnGpsLocationChanged(GpsLocation gpsLocation)
    {
        foreach (var geolocationListener in listeners)
        {
            await geolocationListener.OnGpsLocationChanged(gpsLocation, this);
        }
    }

    
    public override void OnDestroy()
    {
        base.OnDestroy();

        listeners = new HashSet<IGeolocationListener>();
    } 
}
