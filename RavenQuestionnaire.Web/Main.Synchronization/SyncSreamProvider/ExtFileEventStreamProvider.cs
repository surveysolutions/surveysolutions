using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Main.Synchronization.SyncSreamProvider
{
    using Main.Core.Documents;
    using Main.Core.Events;

    class ExtFileEventStreamProvider : ISyncEventStreamProvider
    {
        #region Implementation of ISyncEventStreamProvider

        public IEnumerable<AggregateRootEvent> GetEventStream()
        {
            throw new NotImplementedException();
        }

        public int? GetTotalEventCount()
        {
            return null;
        }

        public SynchronizationType SyncType
        {
            get
            {
                return SynchronizationType.Pull;
            }
        }

        public string ProviderName
        {
            get
            {
                return "File Stream";
            }
        }

        #endregion
    }
}
