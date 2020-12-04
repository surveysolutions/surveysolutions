using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Domain;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    [DisallowConcurrentExecution]
    public class SyncPackagesReprocessorBackgroundJob : IJob
    {
        private readonly IInterviewBrokenPackagesService interviewBrokenPackagesService;
        private readonly SyncPackagesProcessorBackgroundJobSetting syncPackagesProcessorBackgroundJobSetting;
        private readonly IInScopeExecutor inScopeExecutor;
        private readonly ILogger logger;

        public SyncPackagesReprocessorBackgroundJob(
            IInterviewBrokenPackagesService interviewBrokenPackagesService,
            SyncPackagesProcessorBackgroundJobSetting syncPackagesProcessorBackgroundJobSetting,
            IInScopeExecutor inScopeExecutor,
            ILogger logger)
        {
            this.interviewBrokenPackagesService = interviewBrokenPackagesService;
            this.syncPackagesProcessorBackgroundJobSetting = syncPackagesProcessorBackgroundJobSetting;
            this.inScopeExecutor = inScopeExecutor;
            this.logger = logger;
        }

        
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                IReadOnlyCollection<int> packageIds = interviewBrokenPackagesService
                        .GetTopBrokenPackageIdsAllowedToReprocess(syncPackagesProcessorBackgroundJobSetting.SynchronizationBatchCount);
                
                if (packageIds == null || !packageIds.Any()) return Task.CompletedTask;

                logger.Info($"Interview reprocess packages job: Received {packageIds.Count} packages for re-procession. Took {stopwatch.Elapsed:g}.");
                stopwatch.Restart();

                Parallel.ForEach(packageIds,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = syncPackagesProcessorBackgroundJobSetting.SynchronizationParallelExecutorsCount
                    },
                    packageId =>
                    {
                        inScopeExecutor.Execute((serviceLocatorLocal) =>
                        {
                            serviceLocatorLocal.GetInstance<IInterviewBrokenPackagesService>().ReprocessSelectedBrokenPackages(new[] { packageId });
                        });
                    });

                logger.Info($"Interview reprocess job: Processed {packageIds.Count} packages. Took {stopwatch.Elapsed:g}.");
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                logger.Error($"Interview reprocess packages job: FAILED. Reason: {ex.Message} ", ex);
            }

            return Task.CompletedTask; 
        }
    }
}
