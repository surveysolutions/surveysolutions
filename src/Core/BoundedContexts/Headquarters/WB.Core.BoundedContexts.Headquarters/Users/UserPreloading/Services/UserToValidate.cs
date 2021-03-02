using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public class UserToValidate
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public bool IsArchived { get; set; }
        public bool IsSupervisor { get; set; }
        public bool IsInterviewer { get; set; }
        public InWorkspace[] InWorkspaces { get; set; }
    }

    public class InWorkspace
    {
        public string WorkspaceName { get; set; }
        public Guid?  SupervisorId { get; set; }
    }
}
