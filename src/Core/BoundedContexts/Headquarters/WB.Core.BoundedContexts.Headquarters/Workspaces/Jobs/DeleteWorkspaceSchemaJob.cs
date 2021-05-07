using System.ComponentModel;
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
    [DisplayName("Delete Workspace Schema")]
    [Category("Cleanup")]
    public class DeleteWorkspaceSchemaJob : IJob<DeleteWorkspaceJobData>
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

        public async Task Execute(DeleteWorkspaceJobData data, IJobExecutionContext context)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.WorkspaceName))
            {
                logger.LogWarning("Cannot execute delete workspace schema job. Missing workspace name");
                return;
            }

            if (string.IsNullOrWhiteSpace(data.WorkspaceSchema))
            {
                logger.LogWarning("Cannot execute delete workspace schema job. Missing workspace schema");
                return;
            }

            var dbWorkspace = workspaces.GetById(data.WorkspaceName);

            if (dbWorkspace != null)
            {
                logger.LogError("Cannot delete schema for existing workspace");
                return;
            }

            if (!data.WorkspaceSchema.StartsWith(WorkspaceContext.SchemaPrefix))
            {
                logger.LogError("Cannot delete schema {schema} that is not prefixed with {prefix}",
                    data.WorkspaceSchema, WorkspaceContext.SchemaPrefix);
            }

            logger.LogInformation("Deleting workspace {workspace} schema: {schema}", data.WorkspaceName, data.WorkspaceSchema);
            await unitOfWork.Session.Connection.ExecuteAsync($"drop schema if exists \"{data.WorkspaceSchema}\" cascade");
        }
    }
}
