namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public class WorkspaceContext
    {
        public WorkspaceContext(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        public string Name { get; }
        public string DisplayName { get; }

        public static WorkspaceContext From(Workspace workspace) => new WorkspaceContext(workspace.Name, workspace.DisplayName);
    }
}
