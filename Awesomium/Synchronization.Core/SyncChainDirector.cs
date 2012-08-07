using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.SynchronizationFlow;

namespace Synchronization.Core
{
    public class SyncChainDirector : ISyncChainDirector
    {
        private ISynchronizer root;
        private ISynchronizer last;

        #region Implementation of ISyncChainDirector

        public void AddSynchronizer(ISynchronizer synchronizer)
        {
            if (root == null)
            {
                last = root = synchronizer;
                return;
            }
            last = last.SetNext(synchronizer);
        }

        public void Push()
        {
            root.Push();
        }

        public void Pull()
        {
            root.Pull();
        }

        #endregion
    }
}
