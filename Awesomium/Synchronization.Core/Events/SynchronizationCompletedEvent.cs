using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.SynchronizationFlow;

namespace Synchronization.Core.Events
{
    public class SynchronizationCompletedEvent : SynchronizationEvent
    {
        public SynchronizationCompletedEvent(SyncType actionType, SyncDirection direction, bool error, bool canceled) :
            base(actionType, direction, new SyncStatus(100, error, canceled))
        {
        }
    }
}
