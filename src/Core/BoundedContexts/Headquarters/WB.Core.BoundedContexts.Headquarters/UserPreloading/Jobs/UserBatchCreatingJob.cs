using Quartz;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UserBatchCreatingJob : IJob
    {
        IUserBatchCreator UserBatchCreator
        {
            get { return ServiceLocator.Current.GetInstance<IUserBatchCreator>(); }
        }

        public void Execute(IJobExecutionContext context)
        {
            NoTransactionalThreadMarkerManager.MarkCurrentThreadAsNoTransactional();
            try
            {
                UserBatchCreator.CreateUsersFromReadyToBeCreatedQueue();
            }
            finally
            {
                NoTransactionalThreadMarkerManager.ReleaseCurrentThreadAsNoTransactional();
            }
        }
    }
}