using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
