using System;
using System.Collections.Generic;

namespace Synchronization.Core.Events
{
    public class SynchronizationStatisticEventArgs : EventArgs
    {
        public SynchronizationStatisticEventArgs(List<string> info)
        {
            Info = info;
        }

        public List<string> Info { get; private set; }
    }
}
