using Android.Gms.Common;

namespace WB.UI.Tester.CustomServices.Location
{
    public interface IPlayConnectionFailedReceiver
    {
        void OnConnectionFailed(ConnectionResult p0);
    }
}