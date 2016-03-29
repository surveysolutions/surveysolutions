using System.Threading;
using Android.OS;
using Android.App;
using Android.Content;
using MvvmCross.Platform;

namespace WB.UI.Interviewer.Services
{
    [Service]
    public class SyncBgService : Service
    {
        private bool syncStarted = false;

        private SyncServiceBinder binder;

        public void StartSync()
        {
            Mvx.Trace("service started");
        }

        public override IBinder OnBind(Intent intent)
        {
            this.binder = new SyncServiceBinder(this);
            return this.binder;
        }
    }
}