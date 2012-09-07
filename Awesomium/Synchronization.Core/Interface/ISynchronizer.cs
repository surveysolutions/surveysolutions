using System;
using Synchronization.Core.Events;
using Synchronization.Core.SynchronizationFlow;
using System.Collections.Generic;
using Synchronization.Core.Errors;

namespace Synchronization.Core.Interface
{
    public interface ISynchronizer
    {
        void Push(SyncDirection direction = SyncDirection.Up);
        void Pull(SyncDirection direction = SyncDirection.Down);
        void Stop();

        event EventHandler<SynchronizationEvent> SyncProgressChanged;

        string BuildSuccessMessage(SyncType syncAction, SyncDirection direction);
        
        /// <summary>
        /// Make list of current issues which may prevent synchronization
        /// </summary>
        /// <param name="syncAction"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IList<SynchronizationException> CheckSyncIssues(SyncType syncAction, SyncDirection direction);
    }
}