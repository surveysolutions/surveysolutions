using SynchronizationMessages.Discover;

namespace Web.CAPI.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SpotSyncService" in code, svc and config file together.
    public class SpotSyncService : ISpotSync
    {
        public string Process()
        {
            return System.Environment.MachineName;
        }
    }
}
