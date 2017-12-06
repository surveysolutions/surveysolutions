using Android.OS;

namespace WB.UI.Interviewer.Services
{
    public class MapDownloadBackgroundServiceBinder : Binder
    {
        private readonly MapDownloadBackgroundService service;

        public MapDownloadBackgroundServiceBinder(MapDownloadBackgroundService service)
        {
            this.service = service;
        }

        public MapDownloadBackgroundService GetMapDownloadBackgroundService()
        {
            return service;
        }
    }
}