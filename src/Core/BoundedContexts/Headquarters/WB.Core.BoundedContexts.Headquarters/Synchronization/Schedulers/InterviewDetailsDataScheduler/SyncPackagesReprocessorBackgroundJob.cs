using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    [DisallowConcurrentExecution]
    public class SyncPackagesReprocessorBackgroundJob : IJob
    {
        private readonly IServiceLocator serviceLocator;
        private readonly IInterviewBrokenPackagesService interviewBrokenPackagesService;
        private readonly SyncPackagesProcessorBackgroundJobSetting syncPackagesProcessorBackgroundJobSetting;
        private readonly ILogger logger;

        public SyncPackagesReprocessorBackgroundJob(IServiceLocator serviceLocator,
            IInterviewBrokenPackagesService interviewBrokenPackagesService,
            SyncPackagesProcessorBackgroundJobSetting syncPackagesProcessorBackgroundJobSetting,
            ILogger logger)
        {
            this.serviceLocator = serviceLocator;
            this.interviewBrokenPackagesService = interviewBrokenPackagesService;
            this.syncPackagesProcessorBackgroundJobSetting = syncPackagesProcessorBackgroundJobSetting;
            this.logger = logger;
        }

        
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                IReadOnlyCollection<int> packageIds = interviewBrokenPackagesService
                        .GetTopBrokenPackageIdsAllowedToReprocess(syncPackagesProcessorBackgroundJobSetting.SynchronizationBatchCount);
                
                if (packageIds == null || !packageIds.Any()) return;

                logger.Debug($"Interview reprocess packages job: Received {packageIds.Count} packages for re-procession. Took {stopwatch.Elapsed:g}.");
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
    }
}
