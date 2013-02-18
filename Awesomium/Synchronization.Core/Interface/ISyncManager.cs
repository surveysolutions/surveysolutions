using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.Events;
using Synchronization.Core.Errors;
using Synchronization.Core.SynchronizationFlow;

namespace Synchronization.Core.Interface
{
    public interface ISyncManager
    {
        void Push(SyncDirection direction);
        void Pull(SyncDirection direction);
        void Stop();
        /// <summary>
        /// Requests to update active status for all managed synchronizers
        /// </summary>
        void UpdateStatuses();

        IList<ServiceException> CheckSyncIssues(SyncType syncAction, SyncDirection direction);

        event EventHandler<SynchronizationEventArgs> SyncProgressChanged;
        event EventHandler<SynchronizationEventArgs> BgnOfSync;
        event EventHandler<SynchronizationCompletedEventArgs> EndOfSync;
    }
}
