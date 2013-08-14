using Android.Content;
using Xamarin.Geolocation;

namespace CAPI.Android.GeolocationServices
{
    public class GeoService
    {
        private Geolocator geolocator;

        public GeoService(Context context)
        {
            if(geolocator == null)
                geolocator = new Geolocator(context);

            geolocator.DesiredAccuracy = 50;
            geolocator.PositionChanged += OnPositionChanged;
            geolocator.PositionError += OnPositionError;
        }

        private void OnPositionError(object sender, PositionErrorEventArgs e)
        {
            
        }

        private void OnPositionChanged(object sender, PositionEventArgs e)
        {
        }

    }
}