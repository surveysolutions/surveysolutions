namespace WB.UI.Shared.Enumerator.Services;

public interface IGeolocationBackgroundServiceManager
{
    void StartListen(IGeolocationListener geolocationListener);
    void StopListen(IGeolocationListener geolocationListener);
}
