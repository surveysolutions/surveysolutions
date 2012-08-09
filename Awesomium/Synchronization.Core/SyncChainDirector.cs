using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.SynchronizationFlow;

namespace Synchronization.Core
{
    public class SyncChainDirector : ISyncChainDirector
    {
        public SyncChainDirector()
        {
            synchronizerChain = new List<ISynchronizer>();
        }
        public SyncChainDirector(List<ISynchronizer> subStructure)
        {
            synchronizerChain = subStructure;
        }
        private List<ISynchronizer> synchronizerChain;
        #region Implementation of ISyncChainDirector

        public void AddSynchronizer(ISynchronizer synchronizer)
        {
            synchronizerChain.Add(synchronizer);
            SubscribeSynchronizer(synchronizer);
        }

        protected void SubscribeSynchronizer(ISynchronizer synchronizer)
        {
            synchronizer.PullProgressChanged += this.PullProgressChanged;
            synchronizer.PushProgressChanged += this.PushProgressChanged;
        }


        public ISynchronizer ExecuteAction(Action<ISynchronizer> action, IList<Exception> errorList)
        {
            foreach (var synchronizer in synchronizerChain)
            {
                try
                {
                    action(synchronizer);
                    return synchronizer;
                }
                catch (SynchronizationException e)
                {
                    errorList.Add(e);
                }
            }
            return null;
        }

        public event EventHandler<SynchronizationEvent> PushProgressChanged;
        public event EventHandler<SynchronizationEvent> PullProgressChanged;

        #endregion
    }
}
