using System;
using System.Threading;
using System.Threading.Tasks;
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
        }

        public Task<Position> GetPositionAsync(int timeout, CancellationToken cancelToken)
        {
            // we need caching to save a battery if we want to track all coordinates of the answers
            return this.geolocator.GetPositionAsync(timeout, cancelToken);
        }

        public bool IsListening
        {
            get
            {
                return this.geolocator.IsListening;
            }
        }

        public void StopListening()
        {
            geolocator.StopListening();
        }

        public bool IsGeolocationAvailable 
        {
            get { return geolocator.IsGeolocationAvailable; }
        }

        public bool IsGeolocationEnabled
        {
            get { return geolocator.IsGeolocationEnabled; }
        }

        //todo: on suspend stop listening and start again on resume 


        public event EventHandler<PositionErrorEventArgs> PositionError
        {
            add { geolocator.PositionError += value; }
            remove { geolocator.PositionError -= value; }
        }

        public event EventHandler<PositionEventArgs> PositionChanged
        {
            add { geolocator.PositionChanged += value; }
            remove { geolocator.PositionChanged -= value; }
        }

    }
}