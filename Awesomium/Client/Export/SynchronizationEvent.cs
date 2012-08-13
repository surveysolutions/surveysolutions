using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.ExportEvent
{
    public class SynchronizationCompletedEvent : EventArgs
    {
        public SynchronizationCompletedEvent(SyncType actionType)
        {
            ActionType = actionType;
        }

        public SyncType ActionType { get; private set; }
    }

    public enum SyncType
    {
    Push,Pull
    }
}
