using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;

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
            using var jobScope = this.serviceProvider.CreateScope();
            var workspacesService = jobScope.ServiceProvider.GetRequiredService<IWorkspacesCache>();
            var workspaces = workspacesService.GetWorkspaces();

            foreach (var workspace in workspaces)
            {
                using var scope = jobScope.ServiceProvider.CreateWorkspaceScope(workspace);

                try
                {
                    using var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var job = scope.ServiceProvider.GetService(jobType) as IJob;
                    if (job == null)
                        throw new ArgumentNullException(nameof(job));

                    await job.Execute(context);
                    uow.AcceptChanges();
                }
                catch(Exception e)
                {
                    var logger = jobScope.ServiceProvider.GetRequiredService<ILogger<AsyncScopedJobDecorator>>();
                    logger.LogError(e, $"Exception during job {jobType.Name} run in workspace {workspace}");
                }
            }
        }
    }
}
