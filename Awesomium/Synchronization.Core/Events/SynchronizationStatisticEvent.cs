using System;
using System.Collections.Generic;

namespace Synchronization.Core.Events
{
    public class SynchronizationStatisticEvent : EventArgs
    {
        public SynchronizationStatisticEvent(List<string> info)
        {
            Info = info;
        }

        public List<string> Info { get; private set; }
    }
}
