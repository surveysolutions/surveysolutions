using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.Interface;
using Synchronization.Core.Errors;

namespace Synchronization.Core.SynchronizationFlow
{
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

    public struct SyncStatus : ISyncProgressStatus
    {
        int progressPercents;
        SynchronizationException error;
        SyncType actionType;
        SyncDirection direction;

        public SyncStatus(
            SyncType actionType,
            SyncDirection direction,
            int progress, 
            SynchronizationException error)
        {
            this.progressPercents = progress;
            this.error = error;
            this.actionType = actionType;
            this.direction = direction;
        }

        public SyncType ActionType { get { return this.actionType;} }
        public SyncDirection Direction { get { return this.direction;} }
        public int ProgressPercents { get { return this.progressPercents; } }
        public SynchronizationException Error { get { return this.error; } }
    }
}
