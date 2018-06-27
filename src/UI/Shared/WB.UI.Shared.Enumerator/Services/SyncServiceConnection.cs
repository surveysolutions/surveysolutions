using Android.Content;
using Android.OS;

namespace WB.UI.Shared.Enumerator.Services
{
    public interface ISyncServiceHost
    {
        ServiceBinder<SyncBgService> Binder { get; set; }
    }

    public class SyncServiceConnection : Java.Lang.Object, IServiceConnection
    {
        readonly ISyncServiceHost syncServiceHost;

        public SyncServiceConnection(ISyncServiceHost syncServiceHost)
        {
            this.syncServiceHost = syncServiceHost;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            if (service is ServiceBinder<SyncBgService> demoServiceBinder)
            {
                syncServiceHost.Binder = demoServiceBinder;
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
        }
    }
}
