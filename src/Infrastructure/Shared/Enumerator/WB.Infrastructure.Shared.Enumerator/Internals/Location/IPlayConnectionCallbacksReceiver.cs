using Android.OS;

namespace WB.UI.Tester.CustomServices.Location
{
    internal interface IPlayConnectionCallbacksReceiver
    {
        void OnConnected(Bundle p0);
        void OnConnectionSuspended(int cause);
    }
}