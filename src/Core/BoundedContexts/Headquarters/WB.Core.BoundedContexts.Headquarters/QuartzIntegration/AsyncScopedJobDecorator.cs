#nullable enable
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class AsyncScopedJobDecorator : IJob
    {
        private readonly IInScopeExecutor inScopeExecutor;
        private readonly IWorkspacesCache workspacesCache;
        private readonly ILogger<AsyncScopedJobDecorator> logger;
        private readonly Type jobType;

        public AsyncScopedJobDecorator(IInScopeExecutor inScopeExecutor,
            IWorkspacesCache workspacesCache,
            ILogger<AsyncScopedJobDecorator> logger,
            Type jobType)
        {
            this.inScopeExecutor = inScopeExecutor;
            this.workspacesCache = workspacesCache;
            this.logger = logger;
            this.jobType = jobType;
        }

        public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            if (jobType.GetCustomAttribute<NoWorkspaceJobAttribute>() != null)
            {
                await ExecuteJobAsync(context, cancellationToken: cancellationToken);
                return;
            }

            var jobWorkspace = context.MergedJobDataMap.ContainsKey(WorkspaceConstants.QuartzJobKey) 
                ? context.MergedJobDataMap.GetString(WorkspaceConstants.QuartzJobKey)
                : null;

            if (!string.IsNullOrWhiteSpace(jobWorkspace))
            {
                await ExecuteJobAsync(context, jobWorkspace, cancellationToken);
                return;
            }

            foreach (var workspace in this.workspacesCache.AllEnabledWorkspaces())
            {
                await ExecuteJobAsync(context, workspace.Name, cancellationToken);
            }
        }

        private Task ExecuteJobAsync(
            IJobExecutionContext context,
            string? workspace = null,
            CancellationToken cancellationToken = default)
        {
            return inScopeExecutor.ExecuteAsync(async scope =>
            {
                try
                {
                    var job = scope.GetInstance(jobType) as IJob;

                    if (job == null) throw new ArgumentNullException(nameof(job));

                    logger.LogDebug("Executing job {jobType} in workspace {workspace}",
                        jobType.Name, workspace);
                    
                    context.MergedJobDataMap["workspace"] = workspace ?? "";
                    await job.Execute(context, cancellationToken);
                }
                catch (Exception e)
                {
                    if (jobType.GetCustomAttribute<RetryFailedJobAttribute>() != null)
                    {
                        logger.LogError(e, "Exception during job {jobType} run in workspace {workspace}."
                                           + " Scheduling for retry", jobType, workspace);
                        throw new JobExecutionException(e) { RefireImmediately = true };
                    }

                    logger.LogError(e, "Exception during job {jobType} run in workspace {workspace}", 
                        jobType, workspace);

                    if (e.InnerException != null)
                        logger.LogError(e.InnerException, "Inner exception");
                }
            }, workspace);
        }
    }
}
