using Android.Gms.Common;

namespace WB.UI.QuestionnaireTester.CustomServices.Location
{
    public interface IPlayConnectionFailedReceiver
    {
        void OnConnectionFailed(ConnectionResult p0);
    }
}