#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    [DebuggerDisplay("{Name}")]
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
        
        public static Workspace Default { get; } = new Workspace(WorkspaceConstants.DefaultWorkspaceName, "Default Space");
        public static Workspace Admin { get; } = new Workspace(WorkspaceConstants.WorkspaceNames.AdminWorkspaceName, "Server Administration"); 
        public static Workspace UsersWorkspace { get; } = new Workspace(WorkspaceConstants.WorkspaceNames.UsersWorkspaceName, "User"); 
        
        public virtual ISet<WorkspacesUsers> Users { get; set; } = new HashSet<WorkspacesUsers>();
        public virtual DateTime? DisabledAtUtc { get; protected set; }

        protected bool Equals(Workspace other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Workspace) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        
        public virtual WorkspaceContext AsContext() => new (Name, DisplayName, DisabledAtUtc);

        public virtual void Disable()
        {
            if(DisabledAtUtc != null)
                throw new InvalidOperationException("Workspace already disabled");
            if (Name == Default.Name)
                throw new InvalidOperationException($"{Default.Name} workspace can not be disabled");
            this.DisabledAtUtc = DateTime.UtcNow;
        }

        public virtual void Enable()
        {
            if(DisabledAtUtc == null)
                throw new InvalidOperationException("Workspace already enabled");
            this.DisabledAtUtc = null;
        }

        public virtual bool IsDisabled()
        {
            return this.DisabledAtUtc != null;
        }
    }
}
