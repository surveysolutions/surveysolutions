using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.SynchronizationFlow;

namespace Synchronization.Core.Errors
{
    public class CheckPrerequisitesException : SynchronizationException
    {
        public SyncType SychronizationType { get; private set; }

        public CheckPrerequisitesException(string message, SyncType type, Exception innerException)
            : base(message, innerException)
        {
            this.SychronizationType = type;
        }
    }
}
