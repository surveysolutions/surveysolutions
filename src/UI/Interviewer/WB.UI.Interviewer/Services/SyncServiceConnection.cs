using Android.Content;
using Android.OS;
using WB.UI.Interviewer.Activities.Dashboard;

namespace WB.UI.Interviewer.Services
{
    public class SyncServiceConnection : Java.Lang.Object, IServiceConnection
    {
        readonly DashboardActivity activity;

        public SyncServiceConnection(DashboardActivity activity)
        {
            this.activity = activity;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var demoServiceBinder = service as SyncServiceBinder;
            if (demoServiceBinder != null)
            {
                activity.Binder = demoServiceBinder;
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
        }
    }
}