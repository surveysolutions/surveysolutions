using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Jobs
{
    [NoWorkspaceJob]
    [RetryFailedJob]
    [DisallowConcurrentExecution]
    public class DeleteWorkspaceSchemaJob : IJob
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IPlainStorageAccessor<Workspace> workspaces;
        private readonly ILogger<DeleteWorkspaceSchemaJob> logger;

        public DeleteWorkspaceSchemaJob(
            IUnitOfWork unitOfWork, 
            IPlainStorageAccessor<Workspace> workspaces,
            ILogger<DeleteWorkspaceSchemaJob> logger)
        {
            this.unitOfWork = unitOfWork;
            this.workspaces = workspaces;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if(!context.MergedJobDataMap.TryGetValue("workspace", out var workspace))
            {
                logger.LogWarning("Cannot execute delete workspace schema job. Missing workspace name");
                return;
            }

            if (!context.MergedJobDataMap.TryGetValue("schema", out var schema))
            {
                logger.LogWarning("Cannot execute delete workspace schema job. Missing workspace schema");
                return;
            }

            var dbWorkspace = workspaces.GetById(workspace.ToString());

            if (dbWorkspace != null)
            {
                logger.LogError("Cannot delete schema for existing workspace");
                return;
            }

            if (!schema.ToString().StartsWith(WorkspaceContext.SchemaPrefix))
            {
                logger.LogError("Cannot delete schema {schema} that is not prefixed with {prefix}", 
                    schema, WorkspaceContext.SchemaPrefix);
            }

            logger.LogInformation("Deleting workspace {workspace} schema: {schema}", workspace, schema);
            await unitOfWork.Session.Connection.ExecuteAsync($"drop schema if exists \"{schema}\" cascade");
        }

        public static IJobDetail JobDetail()
        {
            return JobBuilder.Create<DeleteWorkspaceSchemaJob>()
                .WithIdentity("DeleteWorkspaceSchema", "Cleanup")
                .StoreDurably()
                .RequestRecovery()
                .WithDescription($"Delete workspace schema")
                .Build();
        }
        
        public static async Task Schedule(IScheduler scheduler, WorkspaceContext workspace)
        {
            var trigger = TriggerBuilder.Create()
                .ForJob(JobDetail().Key)
                .WithIdentity($"Delete {workspace.Name} schema", "Cleanup")
                .UsingJobData("workspace", workspace.Name)
                .UsingJobData("schema", workspace.SchemaName)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }
    }
}
