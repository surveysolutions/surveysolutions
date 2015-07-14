using Android.OS;

namespace WB.UI.Tester.CustomServices.Location
{
    public interface IPlayConnectionCallbacksReceiver
    {
        void OnConnected(Bundle p0);
        void OnConnectionSuspended(int cause);
    }
}