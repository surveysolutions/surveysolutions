using Android.Gms.Common;

namespace WB.Infrastructure.Shared.Enumerator.Internals.Location
{
    internal interface IPlayConnectionFailedReceiver
    {
        void OnConnectionFailed(ConnectionResult p0);
    }
}