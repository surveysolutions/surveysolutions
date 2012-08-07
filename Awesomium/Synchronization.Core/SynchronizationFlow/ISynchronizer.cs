using System;

namespace Synchronization.Core.SynchronizationFlow
{
    public interface ISynchronizer
    {
        ISynchronizer SetNext(ISynchronizer synchronizer);
        void Push();
        void Pull();
        event EventHandler<SynchronizationEvent> PushProgressChanged;
        event EventHandler<SynchronizationEvent> PullProgressChanged;
    }
}