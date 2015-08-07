namespace WB.Infrastructure.Shared.Enumerator.Internals.Location
{
    internal interface IMvxLocationReceiver
    {
        void OnLocationChanged(Android.Locations.Location p0);
    }
}