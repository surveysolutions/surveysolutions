using System;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using MvvmCross.Binding;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ViewSetGpsLocationBinding : BaseBinding<MapView, GpsLocation>
    {
        public ViewSetGpsLocationBinding(MapView view)
            : base(view) {}

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void SetValueToView(MapView view, GpsLocation value)
        {
            if (value == null) 
                view.OnDestroy();
            else
            {
                view.OnCreate(null);
                view.OnStart();
                view.GetMapAsync(new MapReadyHandler {OnSetup = (s, map) => { SetupMap(value, map); }});
            }
        }

        private static void SetupMap(GpsLocation value, GoogleMap map)
        {
            map.Clear();

            var latLng = new LatLng(value.Latitude, value.Longitude);

            MarkerOptions marker = new MarkerOptions();
            marker.SetPosition(latLng);
            map.AddMarker(marker);

            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(latLng);
            builder.Zoom(16);

            CameraPosition cameraPosition = builder.Build();
            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

            map.MoveCamera(cameraUpdate);
        }

        public class MapReadyHandler : Java.Lang.Object, IOnMapReadyCallback
        {
            public EventHandler<GoogleMap> OnSetup;

            public void OnMapReady(GoogleMap map) => this.OnSetup.Invoke(this, map);
        }
    }
}
