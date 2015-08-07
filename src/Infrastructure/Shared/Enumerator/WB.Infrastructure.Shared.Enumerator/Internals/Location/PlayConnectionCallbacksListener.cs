using Android.Gms.Common.Apis;
using Android.OS;

namespace WB.Infrastructure.Shared.Enumerator.Internals.Location
{
    internal class PlayConnectionCallbacksListener
            : Java.Lang.Object
            , IGoogleApiClientConnectionCallbacks
    {
        private readonly IPlayConnectionCallbacksReceiver _owner;

        public PlayConnectionCallbacksListener(IPlayConnectionCallbacksReceiver owner)
        {
            this._owner = owner;
        }

        public void OnConnected(Bundle p0)
        {
            this._owner.OnConnected(p0);
        }

        public void OnConnectionSuspended(int cause)
        {
            this._owner.OnConnectionSuspended(cause);
        }
    }
}