using System;
using Quartz;
using WB.Core.Infrastructure.Modularity;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class AsyncScopedJobDecorator : IJob
    {
        private readonly Type jobType;

        public AsyncScopedJobDecorator(Type jobType)
        {
            this.jobType = jobType;
        }

        public void Execute(IJobExecutionContext context)
        {
            InScopeExecutor.Current.ExecuteActionInScope((serviceLocatorLocal) =>
            {
                var job = serviceLocatorLocal.GetInstance(jobType) as IJob;
                if (job == null)
                    throw new ArgumentNullException(nameof(job));

                job.Execute(context);
            });
        }
    }
}
