using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Jobs
{
    public class DeleteWorkspaceJobData
    {
        public DeleteWorkspaceJobData()
        {
            
        }

        public DeleteWorkspaceJobData(WorkspaceContext workspace)
        {
            WorkspaceName = workspace.Name;
            WorkspaceSchema = workspace.SchemaName;
        }

        public string WorkspaceName { get; set; }
        public string WorkspaceSchema { get; set; }
    }
}
