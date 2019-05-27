using System;
using System.Threading.Tasks;
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

        public Task Execute(IJobExecutionContext context)
        {
            InScopeExecutor.Current.ExecuteActionInScope((serviceLocatorLocal) =>
            {
                var job = serviceLocatorLocal.GetInstance(jobType) as IJob;
                if (job == null)
                    throw new ArgumentNullException(nameof(job));

                job.Execute(context);
            });

            return Task.CompletedTask;
        }
    }
}
