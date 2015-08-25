using Microsoft.Practices.ServiceLocation;
using Quartz;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    [DisallowConcurrentExecution]
    internal class InterviewDetailsBackgroundJob : IJob
    {
        ISyncPackagesProcessor SyncPackagesProcessor
        {
            get { return ServiceLocator.Current.GetInstance<ISyncPackagesProcessor>(); }
        }

        public void Execute(IJobExecutionContext context)
        {
            SyncPackagesProcessor.ProcessNextSyncPackage();
        }
    }
}