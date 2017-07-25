using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UserPreloadingCleanerJob : IJob
    {
        ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<UserPreloadingCleanerJob>();

        IUserPreloadingCleaner UserPreloadingCleaner
        {
            get { return ServiceLocator.Current.GetInstance<IUserPreloadingCleaner>(); }
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                UserPreloadingCleaner.CleanUpInactiveUserPreloadingProcesses();
            }
            catch (Exception ex)
            {
                this.logger.Error($"User Preloading Cleaner Job: FAILED. Reason: {ex.Message} ", ex);
            }
        }
    }
}