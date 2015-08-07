using Android.Gms.Common;

namespace WB.UI.Tester.CustomServices.Location
{
    internal interface IPlayConnectionFailedReceiver
    {
        void OnConnectionFailed(ConnectionResult p0);
    }
}