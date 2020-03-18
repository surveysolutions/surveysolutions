using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class AsyncScopedJobDecorator : IJob
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Type jobType;

        public AsyncScopedJobDecorator(IServiceProvider serviceProvider, Type jobType)
        {
            this.serviceProvider = serviceProvider;
            this.jobType = jobType;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = this.serviceProvider.CreateScope();
            using var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var job = scope.ServiceProvider.GetService(jobType) as IJob;
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            await job.Execute(context);
            uow.AcceptChanges();
        }
    }
}
