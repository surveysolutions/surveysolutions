using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synchronization.Core
{
    public class SynchronizationEvent : EventArgs
    {
        public SynchronizationEvent(int progressPercentage)
        {
            ProgressPercentage = progressPercentage;
        }

        public int ProgressPercentage { get; private set; }
        
    }
}
