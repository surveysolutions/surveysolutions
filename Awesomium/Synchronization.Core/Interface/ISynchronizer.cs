using System;
using Synchronization.Core.Events;
using Synchronization.Core.SynchronizationFlow;
using System.Collections.Generic;
using Synchronization.Core.Errors;

namespace Synchronization.Core.Interface
{
    public interface ISynchronizer
    {
        /// <summary>
        /// Push synchronization
        /// </summary>
        /// <param name="direction"></param>
        /// <returns>Synchronization process identifier</returns>
        Guid Push(SyncDirection direction = SyncDirection.Up);

        /// <summary>
        /// Pull synchronization
        /// </summary>
        /// <param name="direction"></param>
        /// <returns>Synchronization process identifier</returns>
        Guid Pull(SyncDirection direction = SyncDirection.Down);

        void Stop();

        event EventHandler<SynchronizationEventArgs> SyncProgressChanged;

        /// <summary>
        /// Formates a message about successfull completing the sync operation
        /// </summary>
        /// <param name="syncAction"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        string GetSuccessMessage(SyncType syncAction, SyncDirection direction);
        
        /// <summary>
        /// Make list of current issues which may prevent synchronization
        /// </summary>
        /// <param name="syncAction"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IList<ServiceException> CheckSyncIssues(SyncType syncAction, SyncDirection direction);

        /// <summary>
        /// Defines availabiltiy for the synchronizer
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Checks current environment and set/reset IsActive status
        /// </summary>
        void UpdateStatus();
    }
}