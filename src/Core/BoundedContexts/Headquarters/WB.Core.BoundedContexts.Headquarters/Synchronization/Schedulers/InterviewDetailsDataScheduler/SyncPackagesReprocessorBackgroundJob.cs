using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Threading;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    [DisallowConcurrentExecution]
    internal class SyncPackagesReprocessorBackgroundJob : IJob
    {
        ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<SyncPackagesReprocessorBackgroundJob>();
        IInterviewBrokenPackagesService InterviewBrokenPackagesService => ServiceLocator.Current.GetInstance<IInterviewBrokenPackagesService>();
        SyncPackagesProcessorBackgroundJobSetting interviewPackagesJobSetings => ServiceLocator.Current.GetInstance<SyncPackagesProcessorBackgroundJobSetting>();
        IPlainTransactionManager plainTransactionManager => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();


        public async Task Execute(IJobExecutionContext context)
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
            ThreadMarkerManager.MarkCurrentThreadAsNoTransactional();
            try
            {
                return this.plainTransactionManager.ExecuteInQueryTransaction(query);
            }
            finally
            {
                ThreadMarkerManager.RemoveCurrentThreadFromNoTransactional();
            }
        }

        private void ExecuteInPlainTransaction(Action query)
        {
            ThreadMarkerManager.MarkCurrentThreadAsIsolated();
            try
            {
                this.plainTransactionManager.ExecuteInPlainTransaction(query);
            }
            finally
            {
                ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
            }
        }
    }
}
