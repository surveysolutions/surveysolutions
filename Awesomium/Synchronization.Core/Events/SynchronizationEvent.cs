using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.SynchronizationFlow;

namespace Synchronization.Core.Events
{
    public class SynchronizationEvent : EventArgs
    {
        public SynchronizationEvent(SyncStatus syncStatus)
        {
            Status = syncStatus;
        }

        public SyncStatus Status { get; private set; }
    }
}
