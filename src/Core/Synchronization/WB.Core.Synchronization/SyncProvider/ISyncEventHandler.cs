using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events;

namespace WB.Core.Synchronization.SyncProvider
{
    interface ISyncEventHandler
    {
        bool Process(IEnumerable<AggregateRootEvent> stream);
    }
}
