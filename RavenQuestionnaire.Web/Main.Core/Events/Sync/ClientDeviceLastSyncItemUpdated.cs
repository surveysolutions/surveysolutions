using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Events.Sync
{
    [Serializable]
    public class ClientDeviceLastSyncItemUpdated
    {
        [AggregateRootId]
        public Guid Id { set; get; }

        public DateTime ChangeDate { set; get; }

        public long LastSyncItem { set; get; }

    }
}
