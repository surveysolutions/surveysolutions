using System;
using Synchronization.Core.Events;

namespace Synchronization.Core.Interface
{
    public interface ISynchronizer
    {
        void Push(SyncDirection direction = SyncDirection.Up);
        void Pull(SyncDirection direction = SyncDirection.Down);
        void Stop();

        event EventHandler<SynchronizationEvent> SyncProgressChanged;

        string BuildSuccessMessage(SyncType syncAction, SyncDirection direction);
    }
}