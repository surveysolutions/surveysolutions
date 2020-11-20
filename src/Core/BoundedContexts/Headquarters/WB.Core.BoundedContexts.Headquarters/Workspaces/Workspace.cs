#nullable enable
namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public class Workspace
    {
        protected Workspace()
        {
            Name = string.Empty;
            DisplayName = string.Empty;
        }

        public Workspace(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        public virtual string Name { get; set; }
        
        public virtual string DisplayName { get; set; }
    }
}
