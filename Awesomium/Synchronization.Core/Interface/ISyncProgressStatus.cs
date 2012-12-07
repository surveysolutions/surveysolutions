using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.Errors;
using Synchronization.Core.SynchronizationFlow;

namespace Synchronization.Core.Interface
{
    public interface ISyncProgressStatus
    {
        SyncType ActionType { get; }
        SyncDirection Direction { get; }
        int ProgressPercents { get; }
        ServiceException Error { get; }
    }
}
