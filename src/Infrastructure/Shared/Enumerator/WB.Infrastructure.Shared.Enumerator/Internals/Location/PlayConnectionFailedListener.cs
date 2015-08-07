using Android.Gms.Common;
using Android.Gms.Common.Apis;

namespace WB.Infrastructure.Shared.Enumerator.Internals.Location
{
    internal class PlayConnectionFailedListener
        : Java.Lang.Object
        , IGoogleApiClientOnConnectionFailedListener
    {
        private readonly IPlayConnectionFailedReceiver _owner;

        public PlayConnectionFailedListener(IPlayConnectionFailedReceiver owner)
        {
            this._owner = owner;
        }

        public void OnConnectionFailed(ConnectionResult p0)
        {
            this._owner.OnConnectionFailed(p0);
        }
    }
}