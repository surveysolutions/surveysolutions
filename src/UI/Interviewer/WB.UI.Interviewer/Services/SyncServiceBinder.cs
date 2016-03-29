using Android.OS;

namespace WB.UI.Interviewer.Services
{
    public class SyncServiceBinder : Binder
    {
        private readonly SyncBgService service;

        public SyncServiceBinder(SyncBgService service)
        {
            this.service = service;
        }

        public SyncBgService GetSyncService()
        {
            return service;
        }
    }
}