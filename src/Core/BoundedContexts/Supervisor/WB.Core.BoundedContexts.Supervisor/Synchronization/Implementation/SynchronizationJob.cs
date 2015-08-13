using System;
using Quartz;
using WB.Core.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class SynchronizationJob : IJob
    {
        private readonly Func<ISynchronizer> synchronizerFactory;

        public SynchronizationJob(Func<ISynchronizer> synchronizerFactory)
        {
            this.synchronizerFactory = synchronizerFactory;
        }

        public void Execute(IJobExecutionContext context)
        {
            IsolatedThreadManager.MarkCurrentThreadAsIsolated();
            try
            {
                ISynchronizer synchronizer = this.synchronizerFactory.Invoke();
                // we want synchronizer be transient but job is singleton, so we use factory in-between

                synchronizer.Pull();
            }
            finally
            {
                IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
            }
        }
    }
}