using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.SynchronizationFlow;

namespace Synchronization.Core
{
    public interface ISyncChainDirector
    {
        void AddSynchronizer(ISynchronizer synchronizer);
        void Push();
        void Pull();
        event EventHandler<SynchronizationEvent> PushProgressChanged;
        event EventHandler<SynchronizationEvent> PullProgressChanged;
    }
}
