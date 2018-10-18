using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    [DisallowConcurrentExecution]
    internal class SyncPackagesProcessorBackgroundJob : IJob
    {
        private readonly IServiceLocator serviceLocator;
        private readonly ILogger logger;
        private readonly SyncPackagesProcessorBackgroundJobSetting syncPackagesProcessorBackgroundJobSetting;
        private readonly IInterviewPackagesService interviewPackagesService;

        public SyncPackagesProcessorBackgroundJob(IServiceLocator serviceLocator,
            ILogger logger,
            SyncPackagesProcessorBackgroundJobSetting syncPackagesProcessorBackgroundJobSetting,
            IInterviewPackagesService interviewPackagesService)
        {
            this.serviceLocator = serviceLocator;
            this.logger = logger;
            this.syncPackagesProcessorBackgroundJobSetting = syncPackagesProcessorBackgroundJobSetting;
            this.interviewPackagesService = interviewPackagesService;
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                
                IReadOnlyCollection<string> packageIds = interviewPackagesService.GetTopPackageIds(
                    syncPackagesProcessorBackgroundJobSetting.SynchronizationBatchCount);
                
                if (packageIds == null || !packageIds.Any()) return;

                logger.Debug($"Interview packages job: Received {packageIds.Count} packages for procession. Took {stopwatch.Elapsed:g}.");
                stopwatch.Restart();

                Parallel.ForEach(packageIds,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = syncPackagesProcessorBackgroundJobSetting.SynchronizationParallelExecutorsCount
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
