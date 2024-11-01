namespace WB.UI.Shared.Enumerator.Services;

public interface IGeolocationBackgroundServiceManager
{
    Task StartListen(IGeolocationListener geolocationListener);
    void StopListen(IGeolocationListener geolocationListener);
    event EventHandler<LocationReceivedEventArgs> LocationReceived;
}
