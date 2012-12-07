using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.Errors;
using Synchronization.Core.SynchronizationFlow;

namespace Synchronization.Core.Events
{
    public class SynchronizationCompletedEventArgs : SynchronizationEventArgs
    {
        public SynchronizationCompletedEventArgs(SyncStatus status, string log) :
            base(status)
        {
            Log = log;
        }

        public string Log { get; private set; }
    }
}
