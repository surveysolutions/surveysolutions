using System;
using Autofac;
using Quartz;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class AsyncScopedJobDecorator : IJob
    {
        private readonly Type jobType;
        private IContainer container;

        public AsyncScopedJobDecorator(Type jobType, IContainer container)
        {
            this.jobType = jobType;
            this.container = container;
        }

        public void Execute(IJobExecutionContext context)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                var unitOfWork = scope.Resolve<IUnitOfWork>();
                try
                {
                    var job = scope.Resolve(jobType) as IJob;
                    if (job == null) throw new ArgumentNullException(nameof(job));
                    job.Execute(context);

                    unitOfWork.AcceptChanges();
                }
                catch (Exception)
                {
                    unitOfWork.Dispose();
                    throw;
                }
            }
        }
    }
}
