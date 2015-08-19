using Microsoft.Practices.ServiceLocation;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UserPreloadingCleanerJob : IJob
    {
        IUserPreloadingCleaner UserPreloadingCleaner
        {
            get { return ServiceLocator.Current.GetInstance<IUserPreloadingCleaner>(); }
        }

        public void Execute(IJobExecutionContext context)
        {
            IsolatedThreadManager.MarkCurrentThreadAsIsolated();
            NoTransactionalThreadMarkerManager.MarkCurrentThreadAsNoTransactional();
            try
            {
                UserPreloadingCleaner.CleanUpInactiveUserPreloadingProcesses();
            }
            finally
            {
                IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
                NoTransactionalThreadMarkerManager.MarkCurrentThreadAsNoTransactional();
            }
        }
    }
}