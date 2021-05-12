using System;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public class AssignUserWorkspace
    {
        public Workspace Workspace { get; set; }
        public Guid? SupervisorId { get; set; }
    }
}