using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    [DisallowConcurrentExecution]
    internal class SyncPackagesProcessorBackgroundJob : IJob
    {
        private readonly IServiceLocator serviceLocator;

        public SyncPackagesProcessorBackgroundJob(IServiceLocator servicelocator)
        {
            this.serviceLocator = servicelocator;
        }

        public void Execute(IJobExecutionContext context)
        {
            ILogger logger = this.serviceLocator.GetInstance<ILoggerProvider>().GetFor<SyncPackagesProcessorBackgroundJob>();

            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                IReadOnlyCollection<string> packageIds = null;

                serviceLocator.ExecuteActionInScope((serviceLocatorLocal) =>
                {
                    packageIds = serviceLocatorLocal.GetInstance<IInterviewPackagesService>().GetTopPackageIds(
                        serviceLocatorLocal.GetInstance<SyncPackagesProcessorBackgroundJobSetting>().SynchronizationBatchCount);
                });

                if (packageIds == null || !packageIds.Any()) return;

                logger.Debug($"Interview packages job: Received {packageIds.Count} packages for procession. Took {stopwatch.Elapsed:g}.");
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
                            serviceLocatorLocal.GetInstance<IInterviewPackagesService>().ProcessPackage(packageId);
                        });
                    });

                logger.Info($"Interview packages job: Processed {packageIds.Count} packages. Took {stopwatch.Elapsed:g}.");
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                logger.Error($"Interview packages job: FAILED. Reason: {ex.Message} ", ex);
            }
        }
    }
}
