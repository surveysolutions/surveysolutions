﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Threading;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    [DisallowConcurrentExecution]
    internal class InterviewDetailsBackgroundJob : IJob
    {
        ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<InterviewDetailsBackgroundJob>();
        IInterviewPackagesService interviewPackagesService => ServiceLocator.Current.GetInstance<IInterviewPackagesService>();
        InterviewDetailsDataLoaderSettings interviewPackagesJobSetings => ServiceLocator.Current.GetInstance<InterviewDetailsDataLoaderSettings>();
        IPlainTransactionManager plainTransactionManager => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();
        IReadSideStatusService readSideStatusService => ServiceLocator.Current.GetInstance<IReadSideStatusService>();

        public void Execute(IJobExecutionContext context)
        {
            if(this.readSideStatusService.AreViewsBeingRebuiltNow())
                return;

            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                IReadOnlyCollection<string> packageIds = this.ExecuteInQueryTransaction(() =>
                    this.interviewPackagesService.GetTopPackageIds(
                        this.interviewPackagesJobSetings.SynchronizationBatchCount));

                if (packageIds == null || !packageIds.Any()) return;

                this.logger.Debug($"Interview packages job: Received {packageIds.Count} packages for procession. Took {stopwatch.Elapsed:g}.");
                stopwatch.Restart();

                Parallel.ForEach(packageIds,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism =
                            this.interviewPackagesJobSetings.SynchronizationParallelExecutorsCount
                    },
                    packageId =>
                    {
                        this.ExecuteInTransaction(() => this.interviewPackagesService.ProcessPackage(packageId));
                    });

                this.logger.Info($"Interview packages job: Processed {packageIds.Count} packages. Took {stopwatch.Elapsed:g}.");
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                this.logger.Error($"Interview packages job: FAILED. Reason: {ex.Message} ", ex);
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

        private void ExecuteInTransaction(Action query)
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