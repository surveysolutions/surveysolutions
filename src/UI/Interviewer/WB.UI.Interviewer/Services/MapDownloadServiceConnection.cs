using Android.Content;
using Android.OS;
using WB.UI.Interviewer.Activities;

namespace WB.UI.Interviewer.Services
{
    public class MapDownloadServiceConnection : Java.Lang.Object, IServiceConnection
    {
        readonly MapsActivity activity;

        public MapDownloadServiceConnection(MapsActivity activity)
        {
            this.activity = activity;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var mapsBinder = service as MapDownloadBackgroundServiceBinder;
            if (mapsBinder != null)
            {
                activity.Binder = mapsBinder;
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
        }
    }
}