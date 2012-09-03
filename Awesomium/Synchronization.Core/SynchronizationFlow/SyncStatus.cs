using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.Interface;
using Synchronization.Core.Errors;

namespace Synchronization.Core.SynchronizationFlow
{
    /// <summary>
    /// Synchronization action for current node.
    /// </summary>
    public enum SyncType
    {
        /// <summary>
        /// Doucments are being uploaded to another node
        /// </summary>
        Push,
        /// <summary>
        /// Doucments are being downloaded from another node
        /// </summary>
        Pull
    }

    /// <summary>
    /// Direction in documents synxhronization flow.
    /// </summary>
    public enum SyncDirection
    {
        /// <summary>
        /// The way from tablet side to headquater via supervisor
        /// </summary>
        Up,
        /// <summary>
        /// The way from headquater to tablet via supervisor
        /// </summary>
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

        public SyncType ActionType { get { return this.actionType; } }
        public SyncDirection Direction { get { return this.direction; } }
        public int ProgressPercents { get { return this.progressPercents; } }
        public SynchronizationException Error { get { return this.error; } }
    }
}
