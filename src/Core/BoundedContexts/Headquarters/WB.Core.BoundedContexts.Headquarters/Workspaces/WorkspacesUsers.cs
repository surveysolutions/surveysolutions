using System;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public class WorkspacesUsers
    {
        public virtual int Id { get; set; }
        
        public virtual Workspace Workspace { get; set; }

        public virtual Guid UserId { get; set; }
    }
}
