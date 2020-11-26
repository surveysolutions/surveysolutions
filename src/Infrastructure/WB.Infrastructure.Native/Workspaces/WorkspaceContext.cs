namespace WB.Infrastructure.Native.Workspaces
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
    }
}
