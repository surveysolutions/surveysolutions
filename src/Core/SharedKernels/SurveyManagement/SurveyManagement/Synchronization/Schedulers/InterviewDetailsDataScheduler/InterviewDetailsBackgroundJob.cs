using Microsoft.Practices.ServiceLocation;
using Quartz;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    [DisallowConcurrentExecution]
    internal class InterviewDetailsBackgroundJob : IJob
    {
        ISyncPackagesProcessor SyncPackagesProcessor => ServiceLocator.Current.GetInstance<ISyncPackagesProcessor>();
        InterviewDetailsDataLoaderSettings InterviewDetailsDataLoaderSettings => ServiceLocator.Current.GetInstance<InterviewDetailsDataLoaderSettings>();

        public void Execute(IJobExecutionContext context)
        {
            this.SyncPackagesProcessor.ProcessNextSyncPackageBatchInParallel(InterviewDetailsDataLoaderSettings.SynchronizationBatchCount);
        }
    }
}