using Android.OS;

namespace WB.Infrastructure.Shared.Enumerator.Internals.Location
{
    internal interface IPlayConnectionCallbacksReceiver
    {
        void OnConnected(Bundle p0);
        void OnConnectionSuspended(int cause);
    }
}