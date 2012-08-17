using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.SynchronizationFlow;

namespace Synchronization.Core.Events
{
    public class SynchronizationEvent : EventArgs
    {
        public SynchronizationEvent(SyncType syncType, SyncDirection direction, SyncStatus syncStatus)
        {
            Status = syncStatus;
            ActionType = syncType;
            Direction = direction;
        }

        public SyncType ActionType { get; private set; }
        public SyncDirection Direction { get; private set; }
        public SyncStatus Status { get; private set; }
    }

    public enum SyncType
    {
        Push,
        Pull
    }

    public enum SyncDirection
    {
        Up,
        Down
    }
}
