using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Quartz;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    [DisallowConcurrentExecution]
    public class SyncPackagesReprocessorBackgroundJob : IJob
    {
        private IServiceLocator serviceLocator;

        public SyncPackagesReprocessorBackgroundJob(IServiceLocator servicelocator)
        {
            this.serviceLocator = servicelocator;
        }

        
        public void Execute(IJobExecutionContext context)
        {
            ILogger logger = serviceLocator.GetInstance<ILoggerProvider>().GetFor<SyncPackagesReprocessorBackgroundJob>();

            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                IReadOnlyCollection<int> packageIds = null;

                serviceLocator.ExecuteActionInScope((serviceLocatorLocal) =>
                {
                    packageIds = serviceLocator.GetInstance<IInterviewBrokenPackagesService>()
                        .GetTopBrokenPackageIdsAllowedToReprocess(
                            serviceLocator.GetInstance<SyncPackagesProcessorBackgroundJobSetting>()
                                .SynchronizationBatchCount);
                });
                
                if (packageIds == null || !packageIds.Any()) return;

                logger.Debug($"Interview reproces packages job: Received {packageIds.Count} packages for reprocession. Took {stopwatch.Elapsed:g}.");
                stopwatch.Restart();

                Parallel.ForEach(packageIds,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = serviceLocator.GetInstance<SyncPackagesProcessorBackgroundJobSetting>().SynchronizationParallelExecutorsCount
                    },
                    packageId =>
                    {
                        serviceLocator.ExecuteActionInScope((serviceLocatorLocal) =>
                        {
                            serviceLocatorLocal.GetInstance<IInterviewBrokenPackagesService>().ReprocessSelectedBrokenPackages(new[] { packageId });
                        });
                    });

                logger.Info($"Interview packages job: Processed {packageIds.Count} packages. Took {stopwatch.Elapsed:g}.");
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                logger.Error($"Interview reprocess packages job: FAILED. Reason: {ex.Message} ", ex);
            }
        }

        private T ExecuteInQueryTransaction<T>(Func<T> query)
        {
            return query();
        }
    }
}
