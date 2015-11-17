using Microsoft.Practices.ServiceLocation;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;

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
            UserPreloadingCleaner.CleanUpInactiveUserPreloadingProcesses();
        }
    }
}