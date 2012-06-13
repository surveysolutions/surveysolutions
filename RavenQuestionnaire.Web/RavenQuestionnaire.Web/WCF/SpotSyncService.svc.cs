using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SynchronizationMessages.Discover;

namespace DataEntryWCFServer
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
