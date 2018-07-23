using Android.App;
using Android.Content;
using Android.OS;

namespace WB.UI.Shared.Enumerator.Services
{
    public interface ISyncServiceHost<T> where T : Service
    {
        ServiceBinder<T> Binder { get; set; }
    }

    public class SyncServiceConnection<T> : Java.Lang.Object, IServiceConnection where T : Service
    {
        readonly ISyncServiceHost<T> syncServiceHost;

        public SyncServiceConnection(ISyncServiceHost<T> syncServiceHost)
        {
            this.syncServiceHost = syncServiceHost;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            if (service is ServiceBinder<T> serviceBinder)
            {
                syncServiceHost.Binder = serviceBinder;
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
        }
    }
}
