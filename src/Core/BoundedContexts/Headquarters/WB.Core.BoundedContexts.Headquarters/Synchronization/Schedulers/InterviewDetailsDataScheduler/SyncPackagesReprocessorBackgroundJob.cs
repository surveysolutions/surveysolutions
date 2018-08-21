using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    [DisallowConcurrentExecution]
    internal class SyncPackagesReprocessorBackgroundJob : IJob
    {
        ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<SyncPackagesReprocessorBackgroundJob>();
        IInterviewBrokenPackagesService InterviewBrokenPackagesService => ServiceLocator.Current.GetInstance<IInterviewBrokenPackagesService>();
        SyncPackagesProcessorBackgroundJobSetting interviewPackagesJobSetings => ServiceLocator.Current.GetInstance<SyncPackagesProcessorBackgroundJobSetting>();

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                IReadOnlyCollection<int> packageIds = this.ExecuteInQueryTransaction(() =>
                    this.InterviewBrokenPackagesService.GetTopBrokenPackageIdsAllowedToReprocess(this.interviewPackagesJobSetings.SynchronizationBatchCount));

                if (packageIds == null || !packageIds.Any()) return;

                this.logger.Debug($"Interview reproces packages job: Received {packageIds.Count} packages for reprocession. Took {stopwatch.Elapsed:g}.");
                stopwatch.Restart();

                Parallel.ForEach(packageIds,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = this.interviewPackagesJobSetings.SynchronizationParallelExecutorsCount
                    },
                    packageId =>
                    {
                        this.ExecuteInPlainTransaction(() =>
                        {
                            this.InterviewBrokenPackagesService.ReprocessSelectedBrokenPackages(new[] { packageId });
                        });
                    });

                this.logger.Info($"Interview packages job: Processed {packageIds.Count} packages. Took {stopwatch.Elapsed:g}.");
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                this.logger.Error($"Interview reprocess packages job: FAILED. Reason: {ex.Message} ", ex);
            }
        }

        private T ExecuteInQueryTransaction<T>(Func<T> query)
        {
            return query();
        }

        private void ExecuteInPlainTransaction(Action query)
        {
            query();
        }
    }
}
