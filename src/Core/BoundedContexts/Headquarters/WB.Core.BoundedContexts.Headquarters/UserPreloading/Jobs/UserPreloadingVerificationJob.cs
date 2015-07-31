using Quartz;
using WB.Core.Infrastructure.Storage;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UserPreloadingVerificationJob : IJob
    {
        IUserPreloadingVerifier UserPreloadingVerifier
        {
            get { return ServiceLocator.Current.GetInstance<IUserPreloadingVerifier>(); }
        }

        public void Execute(IJobExecutionContext context)
        {
            IsolatedThreadManager.MarkCurrentThreadAsIsolated();
            try
            {
                UserPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();
            }
            finally
            {
                IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
            }
        }
    }
}