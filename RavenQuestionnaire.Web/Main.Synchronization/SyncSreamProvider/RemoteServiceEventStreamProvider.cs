using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Main.Synchronization.SyncSreamProvider
{
    using Main.Core.Events;

    class RemoteServiceEventStreamProvider : IIntSyncEventStreamProvider
    {
        #region Implementation of ISyncEventStreamProvider

        public IEnumerable<AggregateRootEvent> GetEventStream()
        {
            throw new NotImplementedException();
        }

        public int? GetTotalEventCount()
        {
            throw new NotImplementedException();
        }

        public string ProviderName
        {
            get
            {
                return "Remote Service";
            }
        }

        #endregion
    }
}
